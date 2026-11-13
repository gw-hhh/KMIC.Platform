using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Notifications;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;
using Vogen;

namespace Kmic.Notifications.Infrastructure.Persistence;

/// <summary>
/// 本模块全部强类型 ID 的 EF 转换器，由 Vogen 生成注册方法
/// </summary>
[EfCoreConverter<SiteMessageId>]
[EfCoreConverter<NotificationId>]
[EfCoreConverter<DeliveryId>]
[EfCoreConverter<CapturedMessageId>]
[EfCoreConverter<NotificationTemplateId>]
[EfCoreConverter<TemplateVersionId>]
internal sealed partial class NotificationsIdConverters;
