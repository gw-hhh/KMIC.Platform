using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>外部投递渠道（邮件或短信）</summary>
public sealed class DeliveryChannel : Enumeration<DeliveryChannel>
{
    public static readonly DeliveryChannel Email = new("Email", "邮件");

    public static readonly DeliveryChannel Sms = new("Sms", "短信");

    private DeliveryChannel(string code, string name)
        : base(code, name)
    {
    }
}
