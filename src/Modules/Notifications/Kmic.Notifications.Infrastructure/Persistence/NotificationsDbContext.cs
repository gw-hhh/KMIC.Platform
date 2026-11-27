using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence;

internal sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    public override string Module => NotificationsModuleInfo.Name;

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInNotificationsIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => NotificationsModel.Configure(modelBuilder);
}
