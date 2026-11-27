using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Notifications;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;
using Vogen;

namespace Kmic.Notifications.Infrastructure.Persistence;

[EfCoreConverter<SiteMessageId>]
[EfCoreConverter<NotificationId>]
[EfCoreConverter<DeliveryId>]
[EfCoreConverter<CapturedMessageId>]
[EfCoreConverter<NotificationTemplateId>]
[EfCoreConverter<TemplateVersionId>]
internal sealed partial class NotificationsIdConverters;
