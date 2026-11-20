using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Observability;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 本实例看到的容量保护状态，写 Outbox 之前据此放行或拒绝
/// </summary>
/// <remarks>
/// 状态由采样器从共享状态表读来。读不到时保持上一次的状态：过期的样本不能解除已有的熔断
/// </remarks>
internal sealed class MessagingAdmission(TimeProvider time)
{
    private volatile Reading? _latest;

    public AdmissionState State => _latest?.State ?? AdmissionState.Normal;

    /// <summary>
    /// 记下读到的状态；两个时间间隔都按数据库的时钟算，不受本机时钟影响
    /// </summary>
    public void Update(AdmissionState state, TimeSpan sampleAge, TimeSpan? capacityAge) =>
        _latest = new Reading(state, sampleAge, capacityAge, time.GetUtcNow());

    /// <summary>
    /// 量表读取用；还没有读到状态时为空
    /// </summary>
    public AdmissionReading? Read()
    {
        if (_latest is not { } latest)
        {
            return null;
        }

        var sinceRead = time.GetUtcNow() - latest.ReadAt;
        return new AdmissionReading((int)latest.State, latest.SampleAge + sinceRead, latest.CapacityAge + sinceRead);
    }

    /// <summary>
    /// 当前状态下是否允许写消息
    /// </summary>
    /// <param name="origin">发起写入的来源</param>
    /// <param name="exemptFromSoftLine">命令是否声明了软线下仍可执行</param>
    public bool IsAllowed(CallOrigin origin, bool exemptFromSoftLine) => State switch
    {
        AdmissionState.Normal => true,
        AdmissionState.Hard => false,

        // 软线只拦 HTTP 与后台作业：处理消息时产生的消息要让队列排空，定时任务多是清理
        _ => exemptFromSoftLine || origin is not (CallOrigin.Http or CallOrigin.BackgroundJob),
    };

    private sealed record Reading(AdmissionState State, TimeSpan SampleAge, TimeSpan? CapacityAge, DateTimeOffset ReadAt);
}
