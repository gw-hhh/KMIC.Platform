using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Notifications;

namespace Kmic.Notifications.Domain.SiteMessages;

/// <summary>
/// 用户的站内消息
/// </summary>
/// <remarks>
/// 仅更新已读状态；按收件人控制访问，不另设权限点或组织范围限制
/// </remarks>
public sealed class SiteMessage : AggregateRoot<SiteMessageId>
{
    /// <summary>
    /// 标题最大长度（字符数）
    /// </summary>
    public const int TitleMaxLength = 200;

    /// <summary>
    /// 正文最大长度（字符数）
    /// </summary>
    public const int BodyMaxLength = 2000;

    /// <summary>
    /// 类别最大长度（字符数）
    /// </summary>
    public const int CategoryMaxLength = 50;

    /// <summary>
    /// 链接最大长度（字符数）
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
    /// 标为已读，已读消息保留原已读时间
    /// </summary>
    public void MarkRead(DateTimeOffset now) => ReadAt ??= now;

    // 仅允许以单个 / 开头的站内路径；禁止 // 开头、://、反斜杠和控制字符
    private static bool IsInternalPath(string link) =>
        link.Length <= LinkMaxLength
        && link.StartsWith('/')
        && !link.StartsWith("//", StringComparison.Ordinal)
        && !link.Contains('\\', StringComparison.Ordinal)
        && !link.Any(char.IsControl)
        && !link.Contains("://", StringComparison.Ordinal);

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
