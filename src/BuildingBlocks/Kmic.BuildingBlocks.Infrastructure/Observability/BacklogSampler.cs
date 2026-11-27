using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Observability;

internal sealed class BacklogSampler : BackgroundService
{
    private const int FreshIntervals = 3;

    private readonly ModuleFrameworkTables _tables;
    private readonly DataSources _dataSources;
    private readonly SubscriptionCatalog _subscriptions;
    private readonly StartupState _startup;
    private readonly TimeProvider _time;
    private readonly IOptions<DatabaseDiagnosticsOptions> _options;
    private readonly ILogger<BacklogSampler> _logger;
    private readonly ModuleGauges[] _modules;
    private volatile Sample? _latest;

    public BacklogSampler(
        ModuleFrameworkTables tables,
        DataSources dataSources,
        SubscriptionCatalog subscriptions,
        StartupState startup,
        TimeProvider time,
        FrameworkMetrics metrics,
        IEnumerable<IBacklogGauges> moduleGauges,
        IMeterFactory meters,
        IOptions<DatabaseDiagnosticsOptions> options,
        ILogger<BacklogSampler> logger)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(moduleGauges);
        ArgumentNullException.ThrowIfNull(meters);

        _tables = tables;
        _dataSources = dataSources;
        _subscriptions = subscriptions;
        _startup = startup;
        _time = time;
        _options = options;
        _logger = logger;
        metrics.ObserveBacklog(() => Latest);
        _modules = [.. moduleGauges.Select(source => new ModuleGauges(source))];
        foreach (var module in _modules)
        {
            var meter = meters.Create(module.Source.MeterName);
            foreach (var gauge in module.Source.Gauges)
            {
                meter.CreateObservableGauge(gauge.Name, () => ReadingsOf(module, gauge.Name), gauge.Unit, gauge.Description);
            }
        }
    }

    // 超过三个采样间隔未更新视为过期，避免上报陈旧指标
    public BacklogSnapshot? Latest => _latest is { } latest && IsFresh(latest.At) ? latest.Snapshot : null;

    public async Task SampleAsync(CancellationToken ct)
    {
        var snapshot = await ReadAsync(ct);
        _latest = new Sample(snapshot, _time.GetUtcNow());
    }

    // 单个模块采样失败时保留上次有效读数，不影响其他模块
    public async Task SampleModulesAsync(CancellationToken ct)
    {
        foreach (var module in _modules)
        {
            try
            {
                module.Latest = new ModuleSample(await module.Source.ReadAsync(ct), _time.GetUtcNow());
            }
            catch (Exception exception) when (exception is NpgsqlException or InvalidOperationException)
            {
                ObservabilityLog.ModuleBacklogSampleFailed(_logger, exception, module.Source.MeterName);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // 等待启动任务完成（迁移检查通过且路由表注册完毕），确保相关表已就绪
            await _startup.WaitAsync(stoppingToken);
            var modules = _tables.Modules.Count();
            ObservabilityLog.BacklogSamplingStarted(_logger, _options.Value.BacklogSampleInterval.TotalSeconds, modules);
            using var timer = new PeriodicTimer(_options.Value.BacklogSampleInterval, _time);
            do
            {
                await TrySampleAsync(stoppingToken);
                await SampleModulesAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task TrySampleAsync(CancellationToken ct)
    {
        try
        {
            await SampleAsync(ct);
        }
        catch (Exception exception) when (exception is NpgsqlException or InvalidOperationException)
        {
            // 数据库短暂不可用时不中断采样循环；缓存过期后量表会自动停止输出
            ObservabilityLog.BacklogSampleFailed(_logger, exception);
        }
    }

    private IEnumerable<Measurement<double>> ReadingsOf(ModuleGauges module, string gauge) =>
        module.Latest is { } latest && IsFresh(latest.At)
            ? [.. latest.Readings.Where(r => string.Equals(r.Gauge, gauge, StringComparison.Ordinal)).Select(r => new Measurement<double>(r.Value, r.Tags))]
            : [];

    private bool IsFresh(DateTimeOffset at) => _time.GetUtcNow() - at <= _options.Value.BacklogSampleInterval * FreshIntervals;

    private sealed record Sample(BacklogSnapshot Snapshot, DateTimeOffset At);

    private sealed record ModuleSample(IReadOnlyList<BacklogReading> Readings, DateTimeOffset At);

    private sealed class ModuleGauges(IBacklogGauges source)
    {
        private volatile ModuleSample? _latest;

        public IBacklogGauges Source { get; } = source;

        public ModuleSample? Latest
        {
            get => _latest;
            set => _latest = value;
        }
    }

    private async Task<BacklogSnapshot> ReadAsync(CancellationToken ct)
    {
        var database = _dataSources.Transactions(CallOrigin.Inbox);
        var retiring = await ReadRetiringAsync(database, ct);

        var outbox = new List<OutboxBacklog>();
        var inbox = new List<InboxBacklog>();
        var jobs = new List<JobBacklog>();
        foreach (var module in _tables.Modules)
        {
            var tables = _tables.For(module);
            outbox.Add(await ReadOutboxAsync(database, module, tables.Outbox, ct));
            inbox.AddRange(await ReadInboxAsync(database, module, tables.Inbox, retiring, ct));
            jobs.AddRange(await ReadJobsAsync(database, module, tables.Jobs, ct));
        }

        return new BacklogSnapshot(outbox, inbox, jobs, await ReadScheduledAsync(database, ct));
    }

    private static async Task<HashSet<string>> ReadRetiringAsync(NpgsqlDataSource database, CancellationToken ct)
    {
        await using var command = database.CreateCommand(
            $"""
            SELECT DISTINCT "Subscription" FROM {MessageRouteTable.Name} WHERE "Status" = {MessageRouteTable.Retiring}
            """);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var retiring = new HashSet<string>(StringComparer.Ordinal);
        while (await reader.ReadAsync(ct))
        {
            retiring.Add(reader.GetString(0));
        }

        return retiring;
    }

    private static async Task<OutboxBacklog> ReadOutboxAsync(NpgsqlDataSource database, string module, string outbox, CancellationToken ct)
    {
        await using var command = database.CreateCommand(
            $"""
            SELECT count(*), COALESCE(extract(epoch FROM clock_timestamp() - min("OccurredAt")), 0)::float8
            FROM {outbox} WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing})
            """);
        await using var reader = await command.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return new OutboxBacklog(module, reader.GetInt64(0), TimeSpan.FromSeconds(reader.GetDouble(1)));
    }

    private async Task<List<InboxBacklog>> ReadInboxAsync(
        NpgsqlDataSource database, string module, string inbox, HashSet<string> retiring, CancellationToken ct)
    {
        await using var command = database.CreateCommand(
            $"""
            SELECT "Subscription",
                   count(*) FILTER (WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing})),
                   COALESCE(extract(epoch FROM clock_timestamp() - min("ReceivedAt") FILTER (WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}))), 0)::float8,
                   count(*) FILTER (WHERE "Status" = {MessageStatus.Dead}),
                   count(DISTINCT "OrderingKey") FILTER (WHERE "Status" = {MessageStatus.Dead})
            FROM {inbox}
            WHERE "Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}, {MessageStatus.Dead})
            GROUP BY "Subscription"
            """);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var rows = new Dictionary<string, InboxBacklog>(StringComparer.Ordinal);
        while (await reader.ReadAsync(ct))
        {
            var subscription = reader.GetString(0);
            rows[subscription] = new InboxBacklog(
                module, subscription, reader.GetInt64(1), TimeSpan.FromSeconds(reader.GetDouble(2)), reader.GetInt64(3), BlockedKeys(subscription, reader.GetInt64(4)), retiring.Contains(subscription));
        }

        foreach (var declared in _subscriptions.All.Where(s => string.Equals(s.Module, module, StringComparison.Ordinal)))
        {
            rows.TryAdd(declared.Name, new InboxBacklog(module, declared.Name, 0, TimeSpan.Zero, 0, 0, retiring.Contains(declared.Name)));
        }

        return [.. rows.Values];
    }

    // 顺序键仅对有序订阅有效；无序订阅中的死信不会阻塞其他消息
    private long BlockedKeys(string subscription, long deadKeys) =>
        _subscriptions.All.Any(s => s.IsOrdered && string.Equals(s.Name, subscription, StringComparison.Ordinal)) ? deadKeys : 0;

    private static async Task<List<JobBacklog>> ReadJobsAsync(NpgsqlDataSource database, string module, string jobs, CancellationToken ct)
    {
        await using var command = database.CreateCommand(
            $"""
            SELECT "JobType", count(*), extract(epoch FROM clock_timestamp() - min("SubmittedAt"))::float8
            FROM {jobs} WHERE "Status" = {JobStatus.Queued} GROUP BY "JobType"
            """);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var backlog = new List<JobBacklog>();
        while (await reader.ReadAsync(ct))
        {
            backlog.Add(new JobBacklog(module, reader.GetString(0), reader.GetInt64(1), TimeSpan.FromSeconds(reader.GetDouble(2))));
        }

        return backlog;
    }

    private static async Task<List<ScheduledJobFreshness>> ReadScheduledAsync(NpgsqlDataSource database, CancellationToken ct)
    {
        await using var command = database.CreateCommand(
            $"""
            SELECT "JobName", extract(epoch FROM clock_timestamp() - max("FinishedAt"))::float8
            FROM {SchedulingTables.Runs} WHERE "Status" = {ScheduledRunStatus.Succeeded} GROUP BY "JobName"
            """);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var tasks = new List<ScheduledJobFreshness>();
        while (await reader.ReadAsync(ct))
        {
            tasks.Add(new ScheduledJobFreshness(reader.GetString(0), TimeSpan.FromSeconds(reader.GetDouble(1))));
        }

        return tasks;
    }
}
