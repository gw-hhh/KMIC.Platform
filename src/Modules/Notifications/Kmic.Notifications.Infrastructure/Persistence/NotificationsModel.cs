using Kmic.Notifications.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence;

/// <summary>
/// 本模块的实体配置清单：写入上下文和查询上下文共用，新增实体时只改这里
/// </summary>
internal static class NotificationsModel
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SiteMessageConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new DeliveryConfiguration());
        modelBuilder.ApplyConfiguration(new CapturedMessageConfiguration());
    }
}
