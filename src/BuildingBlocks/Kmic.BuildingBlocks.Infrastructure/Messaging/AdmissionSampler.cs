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

/// <summary>
/// 定期判断数据库空间是否紧张，并把结果写进共享状态表，各实例据此拒绝消息写入
/// </summary>
/// <remarks>
/// Api 与 Worker 角色都运行（Worker 停止时也要能判断）。每个周期只有取到咨询锁的一个实例做判断
/// </remarks>
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

    /// <summary>
    /// 判断一次（本周期已经有别的实例判断过时跳过），再读一次共享状态
    /// </summary>
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
            // 数据库暂时不可用也要继续：读不到就保持原状态，过期的样本不能解除已有的熔断
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

        // 刚有别的实例判断过：同一个周期里算两次，连续低于低水位的周期数会多记
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

    // 容量的来源由声明容量设置的模块提供，不一定有登记；它依赖请求级服务，所以每次在新的作用域里取
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

    // 未完成的消息：各模块 Outbox 与 Inbox 里待处理、处理中、死信的载荷字节数
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
