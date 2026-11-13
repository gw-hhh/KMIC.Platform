using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Notifications;

namespace Kmic.Notifications.Domain.SiteMessages;

/// <summary>
/// 站内消息：发给某个用户的一条消息，登录后在页面上看到
/// </summary>
/// <remarks>
/// 只有已读状态会改。访问只按收件人：不按组织范围，也没有权限点，谁的消息谁看
/// </remarks>
public sealed class SiteMessage : AggregateRoot<SiteMessageId>
{
    /// <summary>
    /// 标题最长多少个字符
    /// </summary>
    public const int TitleMaxLength = 200;

    /// <summary>
    /// 正文最长多少个字符
    /// </summary>
    public const int BodyMaxLength = 2000;

    /// <summary>
    /// 类别最长多少个字符
    /// </summary>
    public const int CategoryMaxLength = 50;

    /// <summary>
    /// 链接最长多少个字符
    /// </summary>
    public const int LinkMaxLength = 500;

    private SiteMessage(SiteMessageId id, UserId recipientUserId, NotificationId? notificationId, string title, string body, string? link, string category, DateTimeOffset sentAt)
        : base(id)
    {
        RecipientUserId = recipientUserId;
        NotificationId = notificationId;
        Title = title;
        Body = body;
        Link = link;
        Category = category;
        SentAt = sentAt;
    }

    /// <summary>
    /// 收件人
    /// </summary>
    public UserId RecipientUserId { get; private set; }

    /// <summary>
    /// 产生它的那次发送请求
    /// </summary>
    public NotificationId? NotificationId { get; private set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// 正文，纯文本或受限的 Markdown，前端显示时转义
    /// </summary>
    public string Body { get; private set; }

    /// <summary>
    /// 点击跳转的站内地址，没有就是不能点击
    /// </summary>
    public string? Link { get; private set; }

    /// <summary>
    /// 类别，前端用来分组或加图标
    /// </summary>
    public string Category { get; private set; }

    /// <summary>
    /// 发出的时间
    /// </summary>
    public DateTimeOffset SentAt { get; private set; }

    /// <summary>
    /// 已读的时间，空表示未读
    /// </summary>
    public DateTimeOffset? ReadAt { get; private set; }

    /// <summary>
    /// 创建一条站内消息
    /// </summary>
    /// <param name="recipient">收件人</param>
    /// <param name="notificationId">产生它的发送请求</param>
    /// <param name="title">标题</param>
    /// <param name="body">正文</param>
    /// <param name="link">点击跳转的站内地址，只能是站内相对路径</param>
    /// <param name="category">类别</param>
    /// <param name="now">现在，作为发出的时间</param>
    public static Result<SiteMessage> Create(UserId recipient, NotificationId? notificationId, string title, string body, string? link, string category, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        if (link is not null && !IsInternalPath(link))
        {
            return NotificationsErrors.SiteMessageLinkInvalid;
        }

        return new SiteMessage(
            SiteMessageId.New(),
            recipient,
            notificationId,
            Truncate(title.Trim(), TitleMaxLength),
            Truncate(body.Trim(), BodyMaxLength),
            link,
            Truncate(category.Trim(), CategoryMaxLength),
            now);
    }

    /// <summary>
    /// 标为已读；已经读过的什么也不改
    /// </summary>
    /// <param name="now">现在</param>
    public void MarkRead(DateTimeOffset now) => ReadAt ??= now;

    // 站内地址：以一个斜杠开头，不能是“//”开头（那是协议相对地址）或带协议，也不能带反斜杠和控制字符
    private static bool IsInternalPath(string link) =>
        link.Length <= LinkMaxLength
        && link.StartsWith('/')
        && !link.StartsWith("//", StringComparison.Ordinal)
        && !link.Contains('\\', StringComparison.Ordinal)
        && !link.Any(char.IsControl)
        && !link.Contains("://", StringComparison.Ordinal);

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
