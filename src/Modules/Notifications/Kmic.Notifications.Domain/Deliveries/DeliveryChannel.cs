using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 投递的渠道：站内消息不进投递队列，所以这里只有邮件和短信
/// </summary>
public sealed class DeliveryChannel : Enumeration<DeliveryChannel>
{
    /// <summary>
    /// 邮件
    /// </summary>
    public static readonly DeliveryChannel Email = new("Email", "邮件");

    /// <summary>
    /// 短信
    /// </summary>
    public static readonly DeliveryChannel Sms = new("Sms", "短信");

    private DeliveryChannel(string code, string name)
        : base(code, name)
    {
    }
}
