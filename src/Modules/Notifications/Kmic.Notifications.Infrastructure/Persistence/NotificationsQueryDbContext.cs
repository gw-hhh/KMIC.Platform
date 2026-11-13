using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Application;
using Kmic.Notifications.Contracts;
using Kmic.Notifications.Domain.SiteMessages;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence;

/// <summary>
/// Notifications 模块的查询上下文：不跟踪实体，查询处理器通过 INotificationsQueryDb 使用
/// </summary>
internal sealed class NotificationsQueryDbContext(DbContextOptions<NotificationsQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), INotificationsQueryDb
{
    /// <inheritdoc />
    public override string Module => NotificationsModuleInfo.Name;

    /// <inheritdoc />
    public IQueryable<SiteMessage> SiteMessages => Set<SiteMessage>();

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInNotificationsIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => NotificationsModel.Configure(modelBuilder);
}
