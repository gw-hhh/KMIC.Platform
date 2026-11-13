using System.Reflection;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Application;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Application.SiteMessages.Purge;
using Kmic.Notifications.Contracts;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Infrastructure.Persistence;
using Kmic.Notifications.Infrastructure.Persistence.Repositories;
using Kmic.Notifications.Infrastructure.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Notifications.Infrastructure;

/// <summary>
/// Notifications 模块的服务端注册类
/// </summary>
/// <remarks>
/// 处理器、校验器、EF 配置自动发现；这里只写自动发现做不到的注册
/// </remarks>
public sealed class NotificationsModule : IModule
{
    /// <inheritdoc />
    public string Name => NotificationsModuleInfo.Name;

    /// <inheritdoc />
    public Assembly ApplicationAssembly => typeof(INotificationsQueryDb).Assembly;

    /// <inheritdoc />
    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<NotificationsDbContext, NotificationsQueryDbContext, INotificationsQueryDb>();
        services.AddScoped<ISiteMessageRepository, SiteMessageRepository>();
        services.AddKmicOptions<NotificationsRetentionOptions>(NotificationsRetentionOptions.Section);
        services.AddKmicOptions<NotificationsRenderOptions>(NotificationsRenderOptions.Section);
        services.AddSingleton<ITemplateRenderer, FluidTemplateRenderer>();
    }

    /// <inheritdoc />
    public void ConfigureScheduling(ISchedulingBuilder scheduling)
    {
        ArgumentNullException.ThrowIfNull(scheduling);

        scheduling.Schedule("Notifications.PurgeSiteMessages", "0 20 3 * * ?", run => new PurgeSiteMessagesCommand(run.ScheduledFireTime), o => o.DisplayName("清理过期的站内消息"));
    }
}
