using Kmic.Notifications.Domain.Notifications;
using Kmic.Notifications.Domain.SiteMessages;
using Vogen;

namespace Kmic.Notifications.Infrastructure.Persistence;

/// <summary>
/// 本模块全部强类型 ID 的 EF 转换器，由 Vogen 生成注册方法
/// </summary>
[EfCoreConverter<SiteMessageId>]
[EfCoreConverter<NotificationId>]
internal sealed partial class NotificationsIdConverters;
