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
    /// 高：验证码这类等着用的
    /// </summary>
    public static readonly DeliveryPriority High = new("High", "高");

    private DeliveryPriority(string code, string name)
        : base(code, name)
    {
    }
}
