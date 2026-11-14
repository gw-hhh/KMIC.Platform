using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Domain.Captured;

/// <summary>
/// 捕获模式下保存的邮件或短信内容，供查看核对
/// </summary>
/// <remarks>
/// 开启捕获时保存内容并跳过实际发送；生产环境须显式关闭捕获。各环境使用相同表结构
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
    /// 记录捕获内容
    /// </summary>
    public static CapturedMessage Create(DeliveryId deliveryId, DeliveryChannel channel, string address, DeliveryRole role, string subject, string body, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentNullException.ThrowIfNull(role);

        return new CapturedMessage(CapturedMessageId.New(), deliveryId, channel, address, role, subject, body, now);
    }
}
