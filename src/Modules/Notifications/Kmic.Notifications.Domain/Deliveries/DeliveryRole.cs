using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>邮件收件方式</summary>
public sealed class DeliveryRole : Enumeration<DeliveryRole>
{
    public static readonly DeliveryRole To = new("To", "收件人");

    public static readonly DeliveryRole Cc = new("Cc", "抄送");

    public static readonly DeliveryRole Bcc = new("Bcc", "密送");

    private DeliveryRole(string code, string name)
        : base(code, name)
    {
    }
}
