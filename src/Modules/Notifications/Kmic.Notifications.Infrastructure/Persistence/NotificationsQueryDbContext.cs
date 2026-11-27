using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Application;
using Kmic.Notifications.Contracts;
using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence;

internal sealed class NotificationsQueryDbContext(DbContextOptions<NotificationsQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), INotificationsQueryDb
{
    public override string Module => NotificationsModuleInfo.Name;

    public IQueryable<SiteMessage> SiteMessages => Set<SiteMessage>();

    public IQueryable<NotificationTemplate> Templates => Set<NotificationTemplate>();

    public IQueryable<TemplateVersion> TemplateVersions => Set<TemplateVersion>();

    public IQueryable<Delivery> Deliveries => Set<Delivery>();

    public IQueryable<CapturedMessage> CapturedMessages => Set<CapturedMessage>();

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInNotificationsIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => NotificationsModel.Configure(modelBuilder);
}
