using System.Reflection;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Infrastructure.Background;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Application;
using Kmic.Notifications.Application.Captured.Purge;
using Kmic.Notifications.Application.Channels;
using Kmic.Notifications.Application.Deliveries.Purge;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Application.SiteMessages.Purge;
using Kmic.Notifications.Contracts;
using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Notifications;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;
using Kmic.Notifications.Infrastructure.Channels;
using Kmic.Notifications.Infrastructure.Persistence;
using Kmic.Notifications.Infrastructure.Persistence.Repositories;
using Kmic.Notifications.Infrastructure.Rendering;
using Kmic.Notifications.Infrastructure.Sending;
using Kmic.Notifications.Infrastructure.Templates;
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
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<ICapturedMessageRepository, CapturedMessageRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IStartupTask, SyncTemplateCatalogTask>();
        services.AddScoped<NotificationProcessor>();
        services.AddKmicOptions<NotificationsCaptureOptions>(NotificationsCaptureOptions.Section);
        services.AddKmicOptions<NotificationsRetentionOptions>(NotificationsRetentionOptions.Section);
        services.AddKmicOptions<NotificationsRenderOptions>(NotificationsRenderOptions.Section);
        services.AddSingleton<ITemplateRenderer, FluidTemplateRenderer>();
        services.AddKmicOptions<NotificationsEmailOptions>(NotificationsEmailOptions.Section);
        services.AddSingleton<IEmailSender, MailKitEmailSender>();
        services.AddSingleton<ISmsGateway, UnconfiguredSmsGateway>();
    }

    /// <inheritdoc />
    public void AddWorkerServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKmicOptions<NotificationsDeliveryOptions>(NotificationsDeliveryOptions.Section);
        services.AddKmicLeasedWork<DeliveryWork, DeliveryItem>();
    }

    /// <inheritdoc />
    public void ConfigureMessaging(IMessagingBuilder messaging)
    {
        ArgumentNullException.ThrowIfNull(messaging);

        // 订阅名存进收件箱，发布后不能改
        messaging.Subscribe<SendNotification, ProcessSendNotificationCommand>(
            "Notifications.Process", (message, envelope) => new ProcessSendNotificationCommand(message, envelope.MessageId, envelope.SourceModule, envelope.InitiatorUserId));
        messaging.Subscribe<SendSecretNotification, ProcessSendSecretNotificationCommand>(
            "Notifications.ProcessSecret", (message, envelope) => new ProcessSendSecretNotificationCommand(message, envelope.MessageId, envelope.SourceModule, envelope.InitiatorUserId));
    }

    /// <inheritdoc />
    public void ConfigureScheduling(ISchedulingBuilder scheduling)
    {
        ArgumentNullException.ThrowIfNull(scheduling);

        scheduling.Schedule("Notifications.PurgeSiteMessages", "0 20 3 * * ?", run => new PurgeSiteMessagesCommand(run.ScheduledFireTime), o => o.DisplayName("清理过期的站内消息"));
        scheduling.Schedule("Notifications.PurgeDeliveries", "0 30 3 * * ?", run => new PurgeDeliveriesCommand(run.ScheduledFireTime), o => o.DisplayName("清理过期的投递记录"));
        scheduling.Schedule("Notifications.PurgeCaptured", "0 40 3 * * ?", run => new PurgeCapturedCommand(run.ScheduledFireTime), o => o.DisplayName("清理过期的捕获内容"));
        scheduling.Schedule("Notifications.ClearSecretBodies", "0 10 * * * ?", run => new ClearSecretBodiesCommand(run.ScheduledFireTime), o => o.DisplayName("清空含密通知残留的正文"));
    }
}
