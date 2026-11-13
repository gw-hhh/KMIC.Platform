using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Notifications.Domain.Notifications;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 一次投递：把一条通知用某个渠道发给一个收件人
/// </summary>
/// <remarks>
/// 渲染好的内容随这一行保存，发出去的就是这个内容，重试期间模板被修改也不影响。这一行只在处理发送请求时创建；
/// 之后的状态变化（领取、发送成功、失败退避、人工重试）由投递工作器和管理用例用带条件的单条语句更新，不经聚合，
/// 所以领取用的列和结果列不在聚合的方法里
/// </remarks>
public sealed class Delivery : AggregateRoot<DeliveryId>
{
    private Delivery(
        DeliveryId id,
        NotificationId notificationId,
        DeliveryChannel channel,
        UserId? recipientUserId,
        string recipientKey,
        string addressMasked,
        string? address,
        DeliveryRole role,
        string subject,
        string? body,
        string templateKey,
        string dedupKey,
        int templateVersion,
        string codeHash,
        DeliveryPriority priority,
        DeliveryStatus status,
        DeliverySkipReason? skipReason)
        : base(id)
    {
        NotificationId = notificationId;
        Channel = channel;
        RecipientUserId = recipientUserId;
        RecipientKey = recipientKey;
        AddressMasked = addressMasked;
        Address = address;
        Role = role;
        Subject = subject;
        Body = body;
        TemplateKey = templateKey;
        DedupKey = dedupKey;
        TemplateVersion = templateVersion;
        CodeHash = codeHash;
        Priority = priority;
        Status = status;
        SkipReason = skipReason;
    }

    /// <summary>
    /// 产生它的发送请求
    /// </summary>
    public NotificationId NotificationId { get; private set; }

    /// <summary>
    /// 渠道
    /// </summary>
    public DeliveryChannel Channel { get; private set; } = null!;

    /// <summary>
    /// 收件人，系统里的用户；直接给了地址的收件人为空
    /// </summary>
    public UserId? RecipientUserId { get; private set; }

    /// <summary>
    /// 去重用的收件人键：用户是 u:用户 ID，直接给的地址是 a:地址小写的 SHA-256
    /// </summary>
    public string RecipientKey { get; private set; }

    /// <summary>
    /// 脱敏后的地址，管理页和日志里只显示它
    /// </summary>
    public string AddressMasked { get; private set; }

    /// <summary>
    /// 直接给的地址；收件人是用户时为空，发送时再向 Identity 取最新的联系方式
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// 邮件里的角色
    /// </summary>
    public DeliveryRole Role { get; private set; } = null!;

    /// <summary>
    /// 渲染好的主题
    /// </summary>
    public string Subject { get; private set; }

    /// <summary>
    /// 渲染好的正文；含密模板投递完成后清空
    /// </summary>
    public string? Body { get; private set; }

    /// <summary>
    /// 模板键
    /// </summary>
    public string TemplateKey { get; private set; }

    /// <summary>
    /// 业务给的去重键：同一个模板、收件人、去重键、渠道只有一条投递
    /// </summary>
    public string DedupKey { get; private set; }

    /// <summary>
    /// 实际用的模板版本号，0 表示代码里的默认内容
    /// </summary>
    public int TemplateVersion { get; private set; }

    /// <summary>
    /// 渲染时代码里默认内容的哈希，事后能知道发出的是哪一版默认内容
    /// </summary>
    public string CodeHash { get; private set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public DeliveryPriority Priority { get; private set; } = null!;

    /// <summary>
    /// 状态
    /// </summary>
    public DeliveryStatus Status { get; private set; } = null!;

    /// <summary>
    /// 被跳过的原因，只有已跳过的才有
    /// </summary>
    public DeliverySkipReason? SkipReason { get; private set; }

    /// <summary>
    /// 创建一条等待发送的投递
    /// </summary>
    public static Delivery Queue(
        NotificationId notificationId,
        DeliveryChannel channel,
        UserId? recipientUserId,
        string recipientKey,
        string addressMasked,
        string? address,
        DeliveryRole role,
        string subject,
        string body,
        string templateKey,
        string dedupKey,
        int templateVersion,
        string codeHash,
        DeliveryPriority priority) =>
        Create(notificationId, channel, recipientUserId, recipientKey, addressMasked, address, role, subject, body, templateKey, dedupKey, templateVersion, codeHash, priority, DeliveryStatus.Pending, null);

    /// <summary>
    /// 创建一条已跳过的投递：没有地址或用户已禁用，留一条记录说明为什么没有发
    /// </summary>
    public static Delivery Skip(
        NotificationId notificationId,
        DeliveryChannel channel,
        UserId? recipientUserId,
        string recipientKey,
        DeliveryRole role,
        string subject,
        string templateKey,
        string dedupKey,
        int templateVersion,
        string codeHash,
        DeliveryPriority priority,
        DeliverySkipReason reason) =>
        Create(notificationId, channel, recipientUserId, recipientKey, string.Empty, null, role, subject, null, templateKey, dedupKey, templateVersion, codeHash, priority, DeliveryStatus.Skipped, reason);

    /// <summary>
    /// 创建一条已捕获的投递：非生产环境不真的发送
    /// </summary>
    public static Delivery Capture(
        NotificationId notificationId,
        DeliveryChannel channel,
        UserId? recipientUserId,
        string recipientKey,
        string addressMasked,
        string? address,
        DeliveryRole role,
        string subject,
        string body,
        string templateKey,
        string dedupKey,
        int templateVersion,
        string codeHash,
        DeliveryPriority priority) =>
        Create(notificationId, channel, recipientUserId, recipientKey, addressMasked, address, role, subject, body, templateKey, dedupKey, templateVersion, codeHash, priority, DeliveryStatus.Captured, null);

    private static Delivery Create(
        NotificationId notificationId,
        DeliveryChannel channel,
        UserId? recipientUserId,
        string recipientKey,
        string addressMasked,
        string? address,
        DeliveryRole role,
        string subject,
        string? body,
        string templateKey,
        string dedupKey,
        int templateVersion,
        string codeHash,
        DeliveryPriority priority,
        DeliveryStatus status,
        DeliverySkipReason? skipReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(priority);

        return new Delivery(
            DeliveryId.New(), notificationId, channel, recipientUserId, recipientKey, addressMasked, address, role, subject, body, templateKey, dedupKey, templateVersion, codeHash, priority, status, skipReason);
    }
}
