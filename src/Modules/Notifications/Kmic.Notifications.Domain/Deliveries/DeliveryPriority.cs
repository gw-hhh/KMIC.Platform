using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>投递优先级（高优先级任务会被优先领取）</summary>
public sealed class DeliveryPriority : Enumeration<DeliveryPriority>
{
    public static readonly DeliveryPriority Normal = new("Normal", "普通");

    /// <summary>高优先级（适用于验证码等即时通知）</summary>
    public static readonly DeliveryPriority High = new("High", "高");

    private DeliveryPriority(string code, string name)
        : base(code, name)
    {
    }
}
