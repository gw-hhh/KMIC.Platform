using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Domain.Captured;

/// <summary>
/// 非生产环境捕获的一条内容：本该发出去的邮件或短信，留在这里供查看
/// </summary>
/// <remarks>
/// 只在非生产环境写入；生产环境这张表存在但没有数据，迁移照常建，避免环境之间表结构不同
/// </remarks>
public sealed class CapturedMessage : AggregateRoot<CapturedMessageId>
{
    private CapturedMessage(CapturedMessageId id, DeliveryId deliveryId, DeliveryChannel channel, string address, DeliveryRole role, string subject, string body, DateTimeOffset capturedAt)
        : base(id)
    {
        DeliveryId = deliveryId;
        Channel = channel;
        Address = address;
        Role = role;
        Subject = subject;
        Body = body;
        CapturedAt = capturedAt;
    }

    /// <summary>
    /// 对应的投递
    /// </summary>
    public DeliveryId DeliveryId { get; private set; }

    /// <summary>
    /// 渠道
    /// </summary>
    public DeliveryChannel Channel { get; private set; } = null!;

    /// <summary>
    /// 本该发往的完整地址
    /// </summary>
    public string Address { get; private set; }

    /// <summary>
    /// 邮件里的角色
    /// </summary>
    public DeliveryRole Role { get; private set; } = null!;

    /// <summary>
    /// 主题
    /// </summary>
    public string Subject { get; private set; }

    /// <summary>
    /// 正文
    /// </summary>
    public string Body { get; private set; }

    /// <summary>
    /// 捕获的时间
    /// </summary>
    public DateTimeOffset CapturedAt { get; private set; }

    /// <summary>
    /// 记下一条捕获
    /// </summary>
    public static CapturedMessage Create(DeliveryId deliveryId, DeliveryChannel channel, string address, DeliveryRole role, string subject, string body, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentNullException.ThrowIfNull(role);

        return new CapturedMessage(CapturedMessageId.New(), deliveryId, channel, address, role, subject, body, now);
    }
}
