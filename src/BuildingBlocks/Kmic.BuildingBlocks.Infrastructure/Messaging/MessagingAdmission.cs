using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Observability;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 本地缓存的容量保护状态（采样失败时沿用上次状态，避免误解除熔断）
internal sealed class MessagingAdmission(TimeProvider time)
{
    private volatile Reading? _latest;

    public AdmissionState State => _latest?.State ?? AdmissionState.Normal;

    // 采样间隔以数据库时钟为基准，避免本机时钟偏差
    public void Update(AdmissionState state, TimeSpan sampleAge, TimeSpan? capacityAge) =>
        _latest = new Reading(state, sampleAge, capacityAge, time.GetUtcNow());

    public AdmissionReading? Read()
    {
        if (_latest is not { } latest)
        {
            return null;
        }

        var sinceRead = time.GetUtcNow() - latest.ReadAt;
        return new AdmissionReading((int)latest.State, latest.SampleAge + sinceRead, latest.CapacityAge + sinceRead);
    }

    public bool IsAllowed(CallOrigin origin, bool exemptFromSoftLine) => State switch
    {
        AdmissionState.Normal => true,
        AdmissionState.Hard => false,

        // 软线仅拦截 HTTP 请求与后台作业，允许消息处理衍生消息（确保队列排空）及定时清理任务继续执行
        _ => exemptFromSoftLine || origin is not (CallOrigin.Http or CallOrigin.BackgroundJob),
    };

    private sealed record Reading(AdmissionState State, TimeSpan SampleAge, TimeSpan? CapacityAge, DateTimeOffset ReadAt);
}
