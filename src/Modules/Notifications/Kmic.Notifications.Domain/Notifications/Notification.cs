using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Notifications.Domain.Notifications;

/// <summary>
/// 一次发送请求的记录：处理 SendNotification 时插入一行，之后不再修改
/// </summary>
/// <remarks>
/// 用来从投递或站内消息反查“这是哪次请求产生的”，也是按来源消息幂等的依据：同一条来源消息只处理一次
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
    /// <param name="templateKey">模板键</param>
    /// <param name="templateVersions">各渠道实际用的版本号，JSON</param>
    /// <param name="dedupKey">业务给的去重键</param>
    /// <param name="sourceMessageId">来源消息的 ID</param>
    /// <param name="sourceModule">发出请求的模块</param>
    /// <param name="initiatorUserId">发起这件事的用户</param>
    public static Notification Create(string templateKey, string templateVersions, string dedupKey, Guid sourceMessageId, string sourceModule, UserId? initiatorUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateVersions);
        ArgumentException.ThrowIfNullOrWhiteSpace(dedupKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceModule);

        return new Notification(NotificationId.New(), templateKey, templateVersions, dedupKey, sourceMessageId, sourceModule, initiatorUserId);
    }
}
