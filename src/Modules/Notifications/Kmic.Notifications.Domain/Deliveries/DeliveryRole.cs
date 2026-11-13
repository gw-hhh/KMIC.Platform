using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 邮件收件人的角色
/// </summary>
public sealed class DeliveryRole : Enumeration<DeliveryRole>
{
    /// <summary>
    /// 收件人
    /// </summary>
    public static readonly DeliveryRole To = new("To", "收件人");

    /// <summary>
    /// 抄送
    /// </summary>
    public static readonly DeliveryRole Cc = new("Cc", "抄送");

    /// <summary>
    /// 密送
    /// </summary>
    public static readonly DeliveryRole Bcc = new("Bcc", "密送");

    private DeliveryRole(string code, string name)
        : base(code, name)
    {
    }
}
