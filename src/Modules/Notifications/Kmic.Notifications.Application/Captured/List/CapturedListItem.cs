using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Captured.List;

/// <summary>
/// 捕获列表里的一条，不含正文
/// </summary>
/// <param name="Id">捕获记录 ID</param>
/// <param name="DeliveryId">投递 ID</param>
/// <param name="Channel">渠道</param>
/// <param name="Address">本该发往的完整地址</param>
/// <param name="Role">邮件收件角色</param>
/// <param name="Subject">主题</param>
/// <param name="CapturedAt">捕获时间</param>
public sealed record CapturedListItem(CapturedMessageId Id, DeliveryId DeliveryId, DeliveryChannel Channel, string Address, DeliveryRole Role, string Subject, DateTimeOffset CapturedAt);
