using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Notifications.Domain.Notifications;

/// <summary>
/// 通知发送请求记录，创建后不再修改
/// </summary>
/// <remarks>
/// 通过来源消息 ID 去重，并关联本次产生的投递和站内消息
/// </remarks>
public sealed class Notification : AggregateRoot<NotificationId>
{
    private Notification(NotificationId id, string templateKey, string templateVersions, string dedupKey, Guid sourceMessageId, string sourceModule, UserId? initiatorUserId)
        : base(id)
    {
        TemplateKey = templateKey;
        TemplateVersions = templateVersions;
        DedupKey = dedupKey;
        SourceMessageId = sourceMessageId;
        SourceModule = sourceModule;
        InitiatorUserId = initiatorUserId;
    }

    /// <summary>
    /// 模板键
    /// </summary>
    public string TemplateKey { get; private set; }

    /// <summary>
    /// 本次各渠道实际用的版本号，JSON：渠道编码到版本号，0 表示代码里的默认内容
    /// </summary>
    public string TemplateVersions { get; private set; }

    /// <summary>
    /// 业务给的去重键
    /// </summary>
    public string DedupKey { get; private set; }

    /// <summary>
    /// 来源消息的 ID，唯一
    /// </summary>
    public Guid SourceMessageId { get; private set; }

    /// <summary>
    /// 发出请求的模块
    /// </summary>
    public string SourceModule { get; private set; }

    /// <summary>
    /// 发起这件事的用户，系统发起的为空
    /// </summary>
    public UserId? InitiatorUserId { get; private set; }

    /// <summary>
    /// 记录一次发送请求
    /// </summary>
    public static Notification Create(string templateKey, string templateVersions, string dedupKey, Guid sourceMessageId, string sourceModule, UserId? initiatorUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateVersions);
        ArgumentException.ThrowIfNullOrWhiteSpace(dedupKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceModule);

        return new Notification(NotificationId.New(), templateKey, templateVersions, dedupKey, sourceMessageId, sourceModule, initiatorUserId);
    }
}
