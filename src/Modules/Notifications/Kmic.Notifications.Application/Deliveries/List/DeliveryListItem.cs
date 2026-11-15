using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.List;

/// <summary>
/// 投递列表项，不含正文
/// </summary>
/// <param name="Id">投递 ID</param>
/// <param name="Channel">渠道</param>
/// <param name="Status">状态</param>
/// <param name="SkipReason">跳过的原因，没跳过为空</param>
/// <param name="RecipientUserId">系统内收件人 ID；外部收件人为空</param>
/// <param name="AddressMasked">脱敏后的收件地址</param>
/// <param name="TemplateKey">模板键</param>
/// <param name="TemplateVersion">用的模板版本号，0 表示代码里的默认内容</param>
/// <param name="Attempts">领取次数，人工重试时清零</param>
/// <param name="LastError">最近一次失败的原因</param>
/// <param name="SentAt">发送成功的时间</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="Abandoned">是否已经放弃处理</param>
public sealed record DeliveryListItem(
    DeliveryId Id,
    DeliveryChannel Channel,
    DeliveryStatus Status,
    DeliverySkipReason? SkipReason,
    UserId? RecipientUserId,
    string AddressMasked,
    string TemplateKey,
    int TemplateVersion,
    int Attempts,
    string? LastError,
    DateTimeOffset? SentAt,
    DateTimeOffset CreatedAt,
    bool Abandoned);
