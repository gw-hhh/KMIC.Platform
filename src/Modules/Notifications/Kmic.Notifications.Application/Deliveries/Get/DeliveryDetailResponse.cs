using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Notifications;

namespace Kmic.Notifications.Application.Deliveries.Get;

/// <summary>
/// 一条投递的详情
/// </summary>
/// <param name="Id">投递</param>
/// <param name="NotificationId">产生它的发送请求</param>
/// <param name="Channel">渠道</param>
/// <param name="Role">邮件里的角色</param>
/// <param name="Status">状态</param>
/// <param name="SkipReason">跳过的原因，没跳过为空</param>
/// <param name="RecipientUserId">收件用户，直接给地址的收件人为空</param>
/// <param name="AddressMasked">脱敏后的收件地址</param>
/// <param name="Subject">渲染后的主题</param>
/// <param name="Body">渲染后的正文；含密模板发完或最终失败后为空</param>
/// <param name="TemplateKey">模板键</param>
/// <param name="TemplateVersion">用的模板版本号，0 表示代码里的默认内容</param>
/// <param name="Priority">优先级</param>
/// <param name="Attempts">已经尝试发送几次</param>
/// <param name="NextAttemptAt">下次可以领取的时间</param>
/// <param name="LastError">最近一次失败的原因</param>
/// <param name="ProviderMessageId">渠道返回的消息标识</param>
/// <param name="SentAt">发送成功的时间</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="AbandonedAt">放弃处理的时间，没放弃为空</param>
/// <param name="AbandonReason">放弃处理的原因</param>
public sealed record DeliveryDetailResponse(
    DeliveryId Id,
    NotificationId NotificationId,
    DeliveryChannel Channel,
    DeliveryRole Role,
    DeliveryStatus Status,
    DeliverySkipReason? SkipReason,
    UserId? RecipientUserId,
    string AddressMasked,
    string Subject,
    string? Body,
    string TemplateKey,
    int TemplateVersion,
    DeliveryPriority Priority,
    int Attempts,
    DateTimeOffset? NextAttemptAt,
    string? LastError,
    string? ProviderMessageId,
    DateTimeOffset? SentAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AbandonedAt,
    string? AbandonReason);
