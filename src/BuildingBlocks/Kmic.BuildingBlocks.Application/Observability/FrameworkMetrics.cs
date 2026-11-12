using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 框架指标
/// </summary>
/// <remarks>
/// 通过 IMeterFactory 创建，名称 kmic.{领域}.{名称}，时长用秒。维度只能是有限取值（模块、消息名、订阅名、任务名、
/// 结果、错误码、渠道），不能放用户 ID、资源 ID、追踪 ID 这类取值无限的内容，否则指标的序列数会失控
/// </remarks>
public sealed class FrameworkMetrics
{
    /// <summary>
    /// Meter 的名称，宿主按这个名字订阅
    /// </summary>
    public const string MeterName = "Kmic.Framework";

    private readonly Counter<long> _requests;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _authzDenied;
    private readonly Counter<long> _securityEvents;
    private readonly Counter<long> _securityEventsDropped;
    private readonly Counter<long> _commitOutcomeUnknown;
    private readonly Histogram<double> _messageHandleDuration;
    private readonly Histogram<double> _backgroundJobDuration;
    private readonly Counter<long> _backgroundJobFailures;
    private readonly Histogram<double> _scheduledJobDuration;
    private readonly Counter<long> _scheduledJobFailures;
    private readonly Counter<long> _cacheBroadcasts;
    private readonly Counter<long> _listenerReconnects;
    private readonly Counter<long> _rebuildPaused;
    private readonly Counter<long> _terminalConflicts;
    private readonly Counter<long> _readAuditFailures;
    private readonly Counter<long> _slowQueries;
    private readonly Meter _meter;
    private Func<BacklogSnapshot?>? _backlog;

    /// <summary>
    /// 创建全部指标
    /// </summary>
    public FrameworkMetrics(IMeterFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var meter = _meter = factory.Create(MeterName);
        _requests = meter.CreateCounter<long>("kmic.mediator.requests", "{request}", "命令和查询的执行次数");
        _requestDuration = meter.CreateHistogram<double>("kmic.mediator.request.duration", "s", "命令和查询的执行时长");
        _authzDenied = meter.CreateCounter<long>("kmic.authz.denied", "{denial}", "授权拒绝的次数");
        _securityEvents = meter.CreateCounter<long>("kmic.security_event.count", "{event}", "产生的安全事件数");
        _securityEventsDropped = meter.CreateCounter<long>("kmic.security_event.dropped", "{event}", "队列满了丢弃的安全事件数");
        _commitOutcomeUnknown = meter.CreateCounter<long>("kmic.commit.outcome_unknown", "{commit}", "提交结果无法判定的次数");
        _messageHandleDuration = meter.CreateHistogram<double>("kmic.messaging.handle.duration", "s", "订阅处理一条消息的时长");
        _backgroundJobDuration = meter.CreateHistogram<double>("kmic.background_job.duration", "s", "后台作业从开始到结束的时长");
        _backgroundJobFailures = meter.CreateCounter<long>("kmic.background_job.failures", "{job}", "失败的后台作业数");
        _scheduledJobDuration = meter.CreateHistogram<double>("kmic.scheduled_job.duration", "s", "定时任务一次执行的时长");
        _scheduledJobFailures = meter.CreateCounter<long>("kmic.scheduled_job.failures", "{run}", "失败的定时任务执行次数");
        _cacheBroadcasts = meter.CreateCounter<long>("kmic.cache.invalidation_broadcasts", "{broadcast}", "发出的缓存失效广播数");
        _listenerReconnects = meter.CreateCounter<long>("kmic.cache.listen_reconnects", "{reconnect}", "监听连接断开重连的次数");
        _rebuildPaused = meter.CreateCounter<long>("kmic.readmodel.rebuild_paused", "{run}", "因为找不到的行太多而暂停的重建轮次");
        _terminalConflicts = meter.CreateCounter<long>("kmic.readmodel.terminal_conflicts", "{row}", "已是终态、源端却还返回存在的行数");
        _readAuditFailures = meter.CreateCounter<long>("kmic.audit.read_write_failures", "{failure}", "读审计写不进去的次数");
        _slowQueries = meter.CreateCounter<long>("kmic.db.slow_queries", "{query}", "超过慢查询阈值的 SQL 次数");
    }

    /// <summary>
    /// 一个命令或查询执行完了，结果是“success”或错误码，抛异常时是“exception”
    /// </summary>
    public void Request(string module, string message, string result, TimeSpan elapsed)
    {
        var tags = new TagList { { "module", module }, { "message", message }, { "result", result } };
        _requests.Add(1, tags);
        _requestDuration.Record(elapsed.TotalSeconds, tags);
    }

    /// <summary>
    /// 授权被拒绝
    /// </summary>
    public void AuthzDenied(string reason) => _authzDenied.Add(1, new TagList { { "reason", reason } });

    /// <summary>
    /// 产生了一条安全事件
    /// </summary>
    public void SecurityEvent(string type, string severity) => _securityEvents.Add(1, new TagList { { "type", type }, { "severity", severity } });

    /// <summary>
    /// 安全事件队列满了，丢弃了一条
    /// </summary>
    public void SecurityEventDropped() => _securityEventsDropped.Add(1);

    /// <summary>
    /// 提交结果无法判定，entry 是入口（HTTP、Inbox、后台作业等）
    /// </summary>
    public void CommitOutcomeUnknown(string entry) => _commitOutcomeUnknown.Add(1, new TagList { { "entry", entry } });

    /// <summary>
    /// 订阅处理完一条消息，结果是 processed、failed 或 dead
    /// </summary>
    public void MessageHandled(string subscription, string result, TimeSpan elapsed) =>
        _messageHandleDuration.Record(elapsed.TotalSeconds, new TagList { { "subscription", subscription }, { "result", result } });

    /// <summary>
    /// 后台作业结束，结果是 Succeeded、Failed 或 Cancelled
    /// </summary>
    public void BackgroundJobFinished(string module, string type, string result, TimeSpan elapsed)
    {
        var tags = new TagList { { "module", module }, { "type", type }, { "result", result } };
        _backgroundJobDuration.Record(elapsed.TotalSeconds, tags);
        if (string.Equals(result, "Failed", StringComparison.Ordinal))
        {
            _backgroundJobFailures.Add(1, new TagList { { "module", module }, { "type", type } });
        }
    }

    /// <summary>
    /// 定时任务执行完一次
    /// </summary>
    public void ScheduledJobFinished(string task, bool succeeded, TimeSpan elapsed)
    {
        _scheduledJobDuration.Record(elapsed.TotalSeconds, new TagList { { "task", task } });
        if (!succeeded)
        {
            _scheduledJobFailures.Add(1, new TagList { { "task", task } });
        }
    }

    /// <summary>
    /// 发出了一条缓存失效广播
    /// </summary>
    public void CacheInvalidationBroadcast() => _cacheBroadcasts.Add(1);

    /// <summary>
    /// 监听连接断开后重新连上了
    /// </summary>
    public void ListenerReconnected() => _listenerReconnects.Add(1);

    /// <summary>
    /// 读模型重建因为找不到的行超过阈值而暂停
    /// </summary>
    public void RebuildPaused(string module, string readModel) => _rebuildPaused.Add(1, new TagList { { "module", module }, { "readmodel", readModel } });

    /// <summary>
    /// 终态的行源端还返回存在
    /// </summary>
    public void TerminalConflicts(string module, string readModel, int count) =>
        _terminalConflicts.Add(count, new TagList { { "module", module }, { "readmodel", readModel } });

    /// <summary>
    /// 读审计没有写进去
    /// </summary>
    public void ReadAuditFailed() => _readAuditFailures.Add(1);

    /// <summary>
    /// 一条 SQL 超过了慢查询阈值，module 是发起查询的数据库上下文所在的模块
    /// </summary>
    public void SlowQuery(string module) => _slowQueries.Add(1, new TagList { { "module", module } });

    /// <summary>
    /// 登记积压量表的数据来源，只能登记一次
    /// </summary>
    /// <remarks>
    /// 量表的回调在指标读取线程上执行，不能查库，所以由后台采样器把结果放在内存里，这里只读。
    /// 返回 null 表示还没有可信的数据（刚启动，或采样连续失败后数据已经过期），这时不输出任何数值，
    /// 比输出过期的数字更不容易误导告警
    /// </remarks>
    /// <param name="read">取最新一次采样</param>
    public void ObserveBacklog(Func<BacklogSnapshot?> read)
    {
        ArgumentNullException.ThrowIfNull(read);
        if (Interlocked.CompareExchange(ref _backlog, read, null) is not null)
        {
            throw new InvalidOperationException("积压量表已经登记过了。");
        }

        Gauge("kmic.outbox.pending", "{message}", "待投递的消息条数", s => s.Outbox.Select(o => Measure(o.Pending, ("module", o.Module))));
        Gauge("kmic.outbox.oldest_age", "s", "最老的待投递消息产生至今的时间", s => s.Outbox.Select(o => Measure(o.OldestAge.TotalSeconds, ("module", o.Module))));
        Gauge("kmic.inbox.pending", "{message}", "待处理的消息条数，不含待终结订阅", s => Active(s).Select(i => Measure(i.Pending, Subscription(i))));
        Gauge("kmic.inbox.oldest_age", "s", "最老的待处理消息收到至今的时间，不含待终结订阅", s => Active(s).Select(i => Measure(i.OldestAge.TotalSeconds, Subscription(i))));
        Gauge("kmic.inbox.dead", "{message}", "死信条数", s => s.Inbox.Select(i => Measure(i.Dead, Subscription(i))));
        Gauge("kmic.inbox.blocked_keys", "{key}", "有序订阅里被死信挡住的顺序键个数", s => s.Inbox.Select(i => Measure(i.BlockedKeys, Subscription(i))));
        Gauge("kmic.inbox.retiring_pending", "{message}", "待终结订阅的积压", s => s.Inbox.Where(i => i.Retiring).Select(i => Measure(i.Pending, Subscription(i))));
        Gauge("kmic.background_job.queued", "{job}", "排队中的后台作业数", s => s.Jobs.Select(j => Measure(j.Queued, ("module", j.Module), ("type", j.Type))));
        Gauge("kmic.background_job.oldest_queued", "s", "最老的排队作业已等待的时间", s => s.Jobs.Select(j => Measure(j.OldestQueued.TotalSeconds, ("module", j.Module), ("type", j.Type))));
        Gauge("kmic.scheduled_job.last_success_age", "s", "定时任务距上次成功的时间", s => s.ScheduledJobs.Select(t => Measure(t.LastSuccessAge.TotalSeconds, ("task", t.Task))));
    }

    private static IEnumerable<InboxBacklog> Active(BacklogSnapshot snapshot) => snapshot.Inbox.Where(i => !i.Retiring);

    private static (string Key, string Value)[] Subscription(InboxBacklog inbox) => [("module", inbox.Module), ("subscription", inbox.Subscription)];

    private static Measurement<T> Measure<T>(T value, params (string Key, string Value)[] tags)
        where T : struct
    {
        var list = new TagList();
        foreach (var (key, tagValue) in tags)
        {
            list.Add(key, tagValue);
        }

        return new Measurement<T>(value, list);
    }

    private void Gauge<T>(string name, string unit, string description, Func<BacklogSnapshot, IEnumerable<Measurement<T>>> select)
        where T : struct =>
        _meter.CreateObservableGauge(name, () => _backlog?.Invoke() is { } snapshot ? select(snapshot).ToList() : [], unit, description);
}
