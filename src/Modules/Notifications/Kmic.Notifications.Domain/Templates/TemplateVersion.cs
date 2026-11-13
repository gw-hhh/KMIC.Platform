using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>
/// 模板在某个渠道发布过的一个版本
/// </summary>
/// <remarks>
/// 只增不改：恢复默认内容也不删除历史版本
/// </remarks>
public sealed class TemplateVersion : Entity<TemplateVersionId>
{
    /// <summary>
    /// 主题最长多少个字符
    /// </summary>
    public const int SubjectMaxLength = 200;

    /// <summary>
    /// 正文最长多少个字符
    /// </summary>
    public const int BodyMaxLength = 20000;

    /// <summary>
    /// 备注最长多少个字符
    /// </summary>
    public const int RemarkMaxLength = 500;

    private TemplateVersion(
        TemplateVersionId id, NotificationTemplateId templateId, int version, TemplateChannel channel, string subject, string body, UserId publishedBy, DateTimeOffset publishedAt, string? remark)
        : base(id)
    {
        TemplateId = templateId;
        Version = version;
        Channel = channel;
        Subject = subject;
        Body = body;
        PublishedBy = publishedBy;
        PublishedAt = publishedAt;
        Remark = remark;
    }

    /// <summary>
    /// 所属模板
    /// </summary>
    public NotificationTemplateId TemplateId { get; private set; }

    /// <summary>
    /// 版本号，同一个模板内递增
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// 渠道
    /// </summary>
    public TemplateChannel Channel { get; private set; }

    /// <summary>
    /// 主题；站内消息里是标题
    /// </summary>
    public string Subject { get; private set; }

    /// <summary>
    /// 正文
    /// </summary>
    public string Body { get; private set; }

    /// <summary>
    /// 发布人
    /// </summary>
    public UserId PublishedBy { get; private set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTimeOffset PublishedAt { get; private set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; private set; }

    internal static TemplateVersion Create(
        NotificationTemplateId templateId, int version, TemplateChannel channel, string subject, string body, UserId publishedBy, DateTimeOffset publishedAt, string? remark) =>
        new(TemplateVersionId.New(), templateId, version, channel, subject, body, publishedBy, publishedAt, string.IsNullOrWhiteSpace(remark) ? null : remark.Trim());
}
