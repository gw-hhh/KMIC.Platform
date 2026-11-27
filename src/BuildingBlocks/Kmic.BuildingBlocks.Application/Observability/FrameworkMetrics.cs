using System.Diagnostics;
using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Application.Files;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>框架运行时指标；标签维度须为有限枚举值，禁止包含用户 ID、资源 ID 等高基数数据。</summary>
public sealed class FrameworkMetrics
{
    /// <summary>Meter 名称，宿主按此名称订阅指标。</summary>
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
    private readonly Counter<long> _cacheRequests;
    private readonly Counter<long> _cacheBroadcasts;
    private readonly Counter<long> _listenerReconnects;
    private readonly Counter<long> _uploads;
    private readonly Histogram<long> _uploadBytes;
    private readonly Counter<long> _uploadsRejected;
    private readonly Counter<long> _rebuildPaused;
    private readonly Counter<long> _terminalConflicts;
    private readonly Counter<long> _readAuditFailures;
    private readonly Counter<long> _slowQueries;
    private readonly Counter<long> _admissionRejected;
    private readonly Meter _meter;
    private Func<BacklogSnapshot?>? _backlog;
    private Func<AdmissionReading?>? _admission;

    public FrameworkMetrics(IMeterFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var meter = _meter = factory.Create(MeterName);
        _requests = meter.CreateCounter<long>("kmic.mediator.requests", "{request}", "命令和查询的执行次数");
        _requestDuration = meter.CreateHistogram<double>("kmic.mediator.request.duration", "s", "命令和查询的执行时长");
        _authzDenied = meter.CreateCounter<long>("kmic.authz.denied", "{denial}", "授权拒绝的次数");
        _securityEvents = meter.CreateCounter<long>("kmic.security_event.count", "{event}", "产生的安全事件数");
        _securityEventsDropped = meter.CreateCounter<long>("kmic.security_event.dropped", "{event}", "队列已满时丢弃的安全事件数");
        _commitOutcomeUnknown = meter.CreateCounter<long>("kmic.commit.outcome_unknown", "{commit}", "提交结果无法判定的次数");
        _messageHandleDuration = meter.CreateHistogram<double>("kmic.messaging.handle.duration", "s", "订阅处理一条消息的时长");
        _backgroundJobDuration = meter.CreateHistogram<double>("kmic.background_job.duration", "s", "后台作业从开始到结束的时长");
        _backgroundJobFailures = meter.CreateCounter<long>("kmic.background_job.failures", "{job}", "失败的后台作业数");
        _scheduledJobDuration = meter.CreateHistogram<double>("kmic.scheduled_job.duration", "s", "定时任务一次执行的时长");
        _scheduledJobFailures = meter.CreateCounter<long>("kmic.scheduled_job.failures", "{run}", "失败的定时任务执行次数");
        _cacheRequests = meter.CreateCounter<long>("kmic.cache.requests", "{request}", "缓存读取次数");
        _cacheBroadcasts = meter.CreateCounter<long>("kmic.cache.invalidation_broadcasts", "{broadcast}", "发出的缓存失效广播数");
        _listenerReconnects = meter.CreateCounter<long>("kmic.cache.listen_reconnects", "{reconnect}", "监听连接断开重连的次数");
        _uploads = meter.CreateCounter<long>("kmic.files.uploads", "{upload}", "文件上传次数");
        _uploadBytes = meter.CreateHistogram<long>("kmic.files.upload_bytes", "By", "写入存储的文件大小");
        _uploadsRejected = meter.CreateCounter<long>("kmic.files.rejected", "{upload}", "被拒绝的上传次数");
        _rebuildPaused = meter.CreateCounter<long>("kmic.readmodel.rebuild_paused", "{run}", "因源端缺失行过多而暂停的重建轮次");
        _terminalConflicts = meter.CreateCounter<long>("kmic.readmodel.terminal_conflicts", "{row}", "本地已终态但源端仍返回存在的行数");
        _readAuditFailures = meter.CreateCounter<long>("kmic.audit.read_write_failures", "{failure}", "读取审计写入失败次数");
        _slowQueries = meter.CreateCounter<long>("kmic.db.slow_queries", "{query}", "超过慢查询阈值的 SQL 次数");
        _admissionRejected = meter.CreateCounter<long>("kmic.messaging.admission.rejected", "{write}", "容量保护拒绝或跳过的消息写入次数");
    }

    public void Request(string module, string message, string result, TimeSpan elapsed)
    {
        var tags = new TagList { { "module", module }, { "message", message }, { "result", result } };
        _requests.Add(1, tags);
        _requestDuration.Record(elapsed.TotalSeconds, tags);
    }

    public void AuthzDenied(string reason) => _authzDenied.Add(1, new TagList { { "reason", reason } });

    public void SecurityEvent(string type, string severity) => _securityEvents.Add(1, new TagList { { "type", type }, { "severity", severity } });

    public void SecurityEventDropped() => _securityEventsDropped.Add(1);

    public void CommitOutcomeUnknown(string entry) => _commitOutcomeUnknown.Add(1, new TagList { { "entry", entry } });

    public void MessageHandled(string subscription, string result, TimeSpan elapsed) =>
        _messageHandleDuration.Record(elapsed.TotalSeconds, new TagList { { "subscription", subscription }, { "result", result } });

    public void BackgroundJobFinished(string module, string type, string result, TimeSpan elapsed)
    {
        var tags = new TagList { { "module", module }, { "type", type }, { "result", result } };
        _backgroundJobDuration.Record(elapsed.TotalSeconds, tags);
        if (string.Equals(result, "Failed", StringComparison.Ordinal))
        {
            _backgroundJobFailures.Add(1, new TagList { { "module", module }, { "type", type } });
        }
    }

    public void ScheduledJobFinished(string task, bool succeeded, TimeSpan elapsed)
    {
        _scheduledJobDuration.Record(elapsed.TotalSeconds, new TagList { { "task", task } });
        if (!succeeded)
        {
            _scheduledJobFailures.Add(1, new TagList { { "task", task } });
        }
    }

    public void CacheRequest(string category, bool hit) =>
        _cacheRequests.Add(1, new TagList { { "category", category }, { "result", hit ? "hit" : "miss" } });

    public void FileUploaded(string profile, UploadOutcome outcome, long bytes)
    {
        if (outcome == UploadOutcome.Stored)
        {
            _uploads.Add(1, new TagList { { "profile", profile }, { "result", "stored" } });
            _uploadBytes.Record(bytes, new TagList { { "profile", profile } });
            return;
        }

        _uploads.Add(1, new TagList { { "profile", profile }, { "result", "rejected" } });
        _uploadsRejected.Add(1, new TagList { { "profile", profile }, { "reason", ReasonOf(outcome) } });
    }

    /// <summary>记录因客户端断开、超时或存储写入失败导致的上传中断。</summary>
    public void FileUploadFailed(string profile) => _uploads.Add(1, new TagList { { "profile", profile }, { "result", "failed" } });

    public void CacheInvalidationBroadcast() => _cacheBroadcasts.Add(1);

    public void ListenerReconnected() => _listenerReconnects.Add(1);

    public void RebuildPaused(string module, string readModel) => _rebuildPaused.Add(1, new TagList { { "module", module }, { "readmodel", readModel } });

    public void TerminalConflicts(string module, string readModel, int count) =>
        _terminalConflicts.Add(count, new TagList { { "module", module }, { "readmodel", readModel } });

    public void ReadAuditFailed() => _readAuditFailures.Add(1);

    public void SlowQuery(string module) => _slowQueries.Add(1, new TagList { { "module", module } });

    /// <summary>记录容量保护拦截或跳过的消息写入；<paramref name="state"/> 为 soft/hard，<paramref name="skipped"/> 为 true 表示跳过、false 表示拒绝。</summary>
    public void MessagingAdmissionRejected(string state, string origin, bool skipped) =>
        _admissionRejected.Add(1, new TagList { { "state", state }, { "origin", origin }, { "outcome", skipped ? "skipped" : "rejected" } });

    /// <summary>注册积压指标采样回调，仅允许注册一次。</summary>
    public void ObserveBacklog(Func<BacklogSnapshot?> read)
    {
        ArgumentNullException.ThrowIfNull(read);
        if (Interlocked.CompareExchange(ref _backlog, read, null) is not null)
        {
            throw new InvalidOperationException("积压指标量表已注册，禁止重复登记。");
        }

        Gauge("kmic.outbox.pending", "{message}", "待投递的消息条数", s => s.Outbox.Select(o => Measure(o.Pending, ("module", o.Module))));
        Gauge("kmic.outbox.oldest_age", "s", "最早的待投递消息产生至今时长", s => s.Outbox.Select(o => Measure(o.OldestAge.TotalSeconds, ("module", o.Module))));
        Gauge("kmic.inbox.pending", "{message}", "待处理的消息条数，不含待终结订阅", s => Active(s).Select(i => Measure(i.Pending, Subscription(i))));
        Gauge("kmic.inbox.oldest_age", "s", "最早的待处理消息接收至今时长，不含待终结订阅", s => Active(s).Select(i => Measure(i.OldestAge.TotalSeconds, Subscription(i))));
        Gauge("kmic.inbox.dead", "{message}", "死信条数", s => s.Inbox.Select(i => Measure(i.Dead, Subscription(i))));
        Gauge("kmic.inbox.blocked_keys", "{key}", "有序订阅中被死信阻塞的顺序键数量", s => s.Inbox.Select(i => Measure(i.BlockedKeys, Subscription(i))));
        Gauge("kmic.inbox.retiring_pending", "{message}", "待终结订阅的积压消息数", s => s.Inbox.Where(i => i.Retiring).Select(i => Measure(i.Pending, Subscription(i))));
        Gauge("kmic.background_job.queued", "{job}", "排队中的后台作业数", s => s.Jobs.Select(j => Measure(j.Queued, ("module", j.Module), ("type", j.Type))));
        Gauge("kmic.background_job.oldest_queued", "s", "最早的排队作业已等待时长", s => s.Jobs.Select(j => Measure(j.OldestQueued.TotalSeconds, ("module", j.Module), ("type", j.Type))));
        Gauge("kmic.scheduled_job.last_success_age", "s", "定时任务距上次成功的时长", s => s.ScheduledJobs.Select(t => Measure(t.LastSuccessAge.TotalSeconds, ("task", t.Task))));
    }

    /// <summary>注册容量保护状态回调，仅允许注册一次。</summary>
    public void ObserveAdmission(Func<AdmissionReading?> read)
    {
        ArgumentNullException.ThrowIfNull(read);
        if (Interlocked.CompareExchange(ref _admission, read, null) is not null)
        {
            throw new InvalidOperationException("容量保护指标量表已注册，禁止重复登记。");
        }

        _meter.CreateObservableGauge("kmic.messaging.admission.state", StateReading, "{state}", "消息写入容量保护状态：0 正常、1 软线、2 硬线");
        _meter.CreateObservableGauge("kmic.messaging.admission.sample_age", SampleAgeReading, "s", "容量保护判断结果距上次更新的时长");
        _meter.CreateObservableGauge("kmic.messaging.admission.capacity_age", CapacityAgeReading, "s", "数据库容量配置距上次更新的时长");
    }

    private IEnumerable<Measurement<long>> StateReading() => _admission?.Invoke() is { } r ? [new Measurement<long>(r.State)] : [];

    private IEnumerable<Measurement<double>> SampleAgeReading() => _admission?.Invoke() is { } r ? [new Measurement<double>(r.SampleAge.TotalSeconds)] : [];

    private IEnumerable<Measurement<double>> CapacityAgeReading() =>
        _admission?.Invoke() is { CapacityAge: { } age } ? [new Measurement<double>(age.TotalSeconds)] : [];

    private static string ReasonOf(UploadOutcome outcome) => outcome switch
    {
        UploadOutcome.TooLarge => "too_large",
        UploadOutcome.Empty => "empty",
        UploadOutcome.TypeNotAllowed => "type_not_allowed",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "仅被拒绝的上传存在拒绝原因。"),
    };

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
