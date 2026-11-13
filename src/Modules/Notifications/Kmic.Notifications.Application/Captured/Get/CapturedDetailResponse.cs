using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Captured.Get;

/// <summary>
/// 一条捕获内容的详情
/// </summary>
/// <param name="Id">捕获内容</param>
/// <param name="DeliveryId">对应的投递</param>
/// <param name="Channel">渠道</param>
/// <param name="Address">本该发往的完整地址</param>
/// <param name="Role">邮件里的角色</param>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="CapturedAt">捕获的时间</param>
public sealed record CapturedDetailResponse(CapturedMessageId Id, DeliveryId DeliveryId, DeliveryChannel Channel, string Address, DeliveryRole Role, string Subject, string Body, DateTimeOffset CapturedAt);
