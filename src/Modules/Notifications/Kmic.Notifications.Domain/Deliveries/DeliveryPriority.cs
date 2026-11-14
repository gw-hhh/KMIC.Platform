using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 投递的优先级：高优先级的先被领取
/// </summary>
public sealed class DeliveryPriority : Enumeration<DeliveryPriority>
{
    /// <summary>
    /// 普通
    /// </summary>
    public static readonly DeliveryPriority Normal = new("Normal", "普通");

    /// <summary>
    /// 验证码等需要及时发送的通知
    /// </summary>
    public static readonly DeliveryPriority High = new("High", "高");

    private DeliveryPriority(string code, string name)
        : base(code, name)
    {
    }
}
