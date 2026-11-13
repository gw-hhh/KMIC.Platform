using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence;

/// <summary>
/// Notifications 模块的写入上下文：命令里通过仓储使用
/// </summary>
internal sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options, ModuleDbContextServices services)
    : ModuleDbContext(options, services)
{
    /// <inheritdoc />
    public override string Module => NotificationsModuleInfo.Name;

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInNotificationsIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => NotificationsModel.Configure(modelBuilder);
}
