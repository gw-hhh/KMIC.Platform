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

    /// <summary>
    /// 创建全部指标
    /// </summary>
    public FrameworkMetrics(IMeterFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var meter = factory.Create(MeterName);
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
}
