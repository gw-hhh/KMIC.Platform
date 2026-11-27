using System.Globalization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 定期采样数据库空间并更新共享状态表（Api 与 Worker 均运行，每周期仅持咨询锁的实例采样）
internal sealed class AdmissionSampler : BackgroundService
{
    private readonly ModuleFrameworkTables _tables;
    private readonly DataSources _dataSources;
    private readonly StartupState _startup;
    private readonly MessagingAdmission _admission;
    private readonly IExecutionScopeFactory _scopes;
    private readonly IOptions<MessagingOptions> _options;
    private readonly ILogger<AdmissionSampler> _logger;

    public AdmissionSampler(
        ModuleFrameworkTables tables,
        DataSources dataSources,
        StartupState startup,
        MessagingAdmission admission,
        FrameworkMetrics metrics,
        IOptions<MessagingOptions> options,
        IExecutionScopeFactory scopes,
        ILogger<AdmissionSampler> logger)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        _tables = tables;
        _dataSources = dataSources;
        _startup = startup;
        _admission = admission;
        _scopes = scopes;
        _options = options;
        _logger = logger;
        metrics.ObserveAdmission(admission.Read);
    }

    // 执行一次采样判定并刷新本地状态
    public async Task CycleAsync(CancellationToken ct)
    {
        await EvaluateAsync(ct);
        await RefreshAsync(ct);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _startup.WaitAsync(stoppingToken);
            using var timer = new PeriodicTimer(_options.Value.Admission.SampleInterval);
            do
            {
                await TryCycleAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task TryCycleAsync(CancellationToken ct)
    {
        try
        {
            await CycleAsync(ct);
        }
        catch (Exception exception) when (exception is NpgsqlException or InvalidOperationException)
        {
            // 数据库暂时不可用时继续运行，采样失败则保持原状态，避免用过期样本解除熔断
            MessagingLog.AdmissionSampleFailed(_logger, exception);
        }
    }

    private async Task EvaluateAsync(CancellationToken ct)
    {
        var settings = _options.Value.Admission;
        await using var connection = await _dataSources.Transactions(CallOrigin.Inbox).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        if (!await TryLockAsync(connection, transaction, ct))
        {
            return;
        }

        var previous = await ReadRowAsync(connection, transaction, ct);

        // 当前周期已由其他实例完成判定，跳过重复计算以避免多记连续低水位周期数
        if (previous is { SampleAge: { } age } && age < settings.SampleInterval * 0.9)
        {
            return;
        }

        var before = previous?.State ?? AdmissionState.Normal;
        var databaseBytes = await ScalarAsync(connection, transaction, "SELECT pg_database_size(current_database())", ct);
        var unfinishedBytes = await UnfinishedBytesAsync(connection, transaction, ct);
        var capacity = await ReadCapacityAsync(ct);
        var decision = AdmissionPolicy.Next(
            new AdmissionPolicy.Input(before, previous?.LowWaterPeriods ?? 0, databaseBytes, unfinishedBytes, capacity?.Bytes),
            settings);
        await WriteRowAsync(connection, transaction, decision, databaseBytes, unfinishedBytes, capacity, ct);
        await transaction.CommitAsync(ct);

        if (decision.State != before)
        {
            MessagingLog.AdmissionChanged(_logger, before, decision.State, databaseBytes, unfinishedBytes, capacity?.Bytes);
        }
    }

    // 容量来源由声明相关配置的模块提供（可能未注册）；因依赖请求级服务，每次须在独立作用域内解析
    private async Task<MessagingCapacity?> ReadCapacityAsync(CancellationToken ct)
    {
        await using var scope = _scopes.CreateScope(CallOrigin.Inbox, CurrentUser.System());
        return scope.ServiceProvider.GetService<IMessagingCapacitySource>() is { } source ? await source.ReadAsync(ct) : null;
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        await using var connection = await _dataSources.Transactions(CallOrigin.Inbox).OpenConnectionAsync(ct);
        if (await ReadRowAsync(connection, null, ct) is { } row)
        {
            _admission.Update(row.State, row.SampleAge ?? TimeSpan.Zero, row.CapacityAge);
        }
    }

    private static async Task<bool> TryLockAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand("SELECT pg_try_advisory_xact_lock(@key)", connection, transaction);
        command.Parameters.AddWithValue("key", AdmissionTable.LockKey);
        return (bool)(await command.ExecuteScalarAsync(ct))!;
    }

    private static async Task<long> ScalarAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        return Convert.ToInt64(await command.ExecuteScalarAsync(ct), CultureInfo.InvariantCulture);
    }

    // 未完成消息：各模块 Outbox 与 Inbox 中待处理、处理中及死信消息的载荷字节总和
    private async Task<long> UnfinishedBytesAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken ct)
    {
        var total = 0L;
        foreach (var module in _tables.Modules)
        {
            var names = _tables.For(module);
            foreach (var table in new[] { names.Outbox, names.Inbox })
            {
                total += await ScalarAsync(
                    connection,
                    transaction,
                    $"""
                    SELECT COALESCE(sum(pg_column_size("Payload")), 0)::bigint FROM {table}
                    WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}, {MessageStatus.Dead})
                    """,
                    ct);
            }
        }

        return total;
    }

    private static async Task<Row?> ReadRowAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""
            SELECT "State", "LowWaterPeriods",
                   extract(epoch FROM clock_timestamp() - "SampledAt")::float8,
                   extract(epoch FROM clock_timestamp() - "CapacityUpdatedAt")::float8
            FROM {AdmissionTable.Name} WHERE "Id" = 1
            """,
            connection,
            transaction);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        var capacityAge = await reader.IsDBNullAsync(3, ct) ? (TimeSpan?)null : TimeSpan.FromSeconds(reader.GetDouble(3));
        return new Row((AdmissionState)reader.GetInt16(0), reader.GetInt32(1), TimeSpan.FromSeconds(reader.GetDouble(2)), capacityAge);
    }

    private static async Task WriteRowAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        AdmissionPolicy.Decision decision,
        long databaseBytes,
        long unfinishedBytes,
        MessagingCapacity? capacity,
        CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""
            INSERT INTO {AdmissionTable.Name}
                ("Id", "State", "LowWaterPeriods", "DatabaseBytes", "UnfinishedBytes", "CapacityBytes", "CapacityUpdatedAt", "SampledAt")
            VALUES (1, @state, @low, @database, @unfinished, @capacity, @capacityAt, clock_timestamp())
            ON CONFLICT ("Id") DO UPDATE SET
                "State" = EXCLUDED."State", "LowWaterPeriods" = EXCLUDED."LowWaterPeriods",
                "DatabaseBytes" = EXCLUDED."DatabaseBytes", "UnfinishedBytes" = EXCLUDED."UnfinishedBytes",
                "CapacityBytes" = EXCLUDED."CapacityBytes", "CapacityUpdatedAt" = EXCLUDED."CapacityUpdatedAt",
                "SampledAt" = EXCLUDED."SampledAt"
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("state", (short)decision.State);
        command.Parameters.AddWithValue("low", decision.LowWaterPeriods);
        command.Parameters.AddWithValue("database", databaseBytes);
        command.Parameters.AddWithValue("unfinished", unfinishedBytes);
        command.Parameters.Add(new NpgsqlParameter<long?>("capacity", capacity?.Bytes));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset?>("capacityAt", capacity?.UpdatedAt));
        await command.ExecuteNonQueryAsync(ct);
    }

    private sealed record Row(AdmissionState State, int LowWaterPeriods, TimeSpan? SampleAge, TimeSpan? CapacityAge);
}
