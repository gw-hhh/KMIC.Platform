using System.Reflection;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Dictionaries;
using Kmic.BuildingBlocks.Application.Errors;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddKmicApplication(this IServiceCollection services, IReadOnlyCollection<Assembly> applicationAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(applicationAssemblies);

        // 各目录在注册时交叉校验，声明有误时阻止宿主启动
        var useCases = UseCaseCatalog.FromAssemblies(applicationAssemblies);
        var permissions = PermissionCatalog.FromAssemblies(applicationAssemblies);
        var settings = SystemSettingCatalog.FromAssemblies(applicationAssemblies);
        var types = applicationAssemblies.Distinct().SelectMany(a => a.GetTypes()).ToList();
        var resourceActions = services.AddResourceAuthorization(types);
        services.AddDomainEventHandlers(types);
        var jobs = BackgroundJobCatalog.FromAssemblies(applicationAssemblies);
        var uploadProfiles = UploadProfileCatalog.FromAssemblies(applicationAssemblies);
        permissions.Verify(useCases, [.. resourceActions, .. settings.PermissionReferences, .. jobs.PermissionReferences, .. uploadProfiles.PermissionReferences]);

        services.AddSingleton(jobs);
        services.AddSingleton(uploadProfiles);
        foreach (var job in jobs.All.Where(j => j.HandlerType is not null))
        {
            services.AddScoped(job.HandlerInterface, job.HandlerType!);
        }

        services.AddSingleton(permissions);
        services.AddSingleton(ErrorCatalog.FromAssemblies(ErrorAssemblies(applicationAssemblies)));
        services.AddSingleton(settings);
        services.AddSingleton(DataScopeObjectCatalog.FromAssemblies(applicationAssemblies));
        services.AddSingleton(DictionaryTypeCatalog.FromAssemblies(applicationAssemblies));
        services.AddSingleton(PreferenceCatalog.FromAssemblies(applicationAssemblies));
        services.AddSingleton(NumberRuleCatalog.FromAssemblies(applicationAssemblies));
        services.AddSingleton(IntegrationMessageCatalog.FromAssemblies(applicationAssemblies));
        services.AddSingleton(NotificationTemplateCatalog.FromAssemblies(applicationAssemblies));
        services.AddOptions<CursorOptions>();
        services.AddSingleton<ICursorProtector, HmacCursorProtector>();
        return services.AddKmicApplication(useCases, applicationAssemblies);
    }

    public static IServiceCollection AddKmicSubscriptions(this IServiceCollection services, string module, Action<IMessagingBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(module);
        ArgumentNullException.ThrowIfNull(configure);

        var subscriptions = new ModuleSubscriptions(module);
        configure(subscriptions);
        services.AddSingleton(subscriptions);
        return services;
    }

    public static IServiceCollection AddKmicScheduling(this IServiceCollection services, string module, Action<ISchedulingBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(module);
        ArgumentNullException.ThrowIfNull(configure);

        var schedules = new ModuleSchedules(module);
        configure(schedules);
        services.AddSingleton(schedules);
        return services;
    }

    // 测试程序集包含刻意构造的错误声明样例，因此测试仅注册所需消息
    internal static IServiceCollection AddKmicApplication(
        this IServiceCollection services,
        UseCaseCatalog catalog,
        IReadOnlyCollection<Assembly> validatorAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(catalog);
        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<PipelineOptions>();
        services.AddMetrics();
        services.TryAddSingleton<FrameworkMetrics>();
        services.AddScoped<ExecutionScope>();
        services.AddScoped<IExecutionContext>(sp => sp.GetRequiredService<ExecutionScope>());
        services.AddSingleton<IExecutionScopeFactory, ExecutionScopeFactory>();
        services.AddScoped<ISender, Sender>();
        services.AddScoped<ICommandSender>(sp => sp.GetRequiredService<ISender>());
        services.AddScoped<IQuerySender>(sp => sp.GetRequiredService<ISender>());
        services.AddScoped<IContractQuerySender, ContractQuerySender>();
        services.AddScoped<IDataScope, DataScope>();
        services.AddScoped<IEntityFinder, EntityFinder>();
        services.AddScoped<RequestMetadata>();
        services.AddOptions<BusinessTimeOptions>();
        services.TryAddSingleton<IBusinessCalendar>(sp => new BusinessCalendar(
            sp.GetRequiredService<TimeProvider>(),
            TimeZoneInfo.FindSystemTimeZoneById(sp.GetRequiredService<IOptions<BusinessTimeOptions>>().Value.BusinessTimeZone)));
        services.TryAddSingleton(IntegrationMessageCatalog.Empty);
        services.TryAddSingleton(BackgroundJobCatalog.Empty);
        services.TryAddSingleton(UploadProfileCatalog.Empty);
        services.AddScoped<IntegrationEventPublisher>();
        services.AddScoped<IIntegrationEventPublisher>(sp => sp.GetRequiredService<IntegrationEventPublisher>());
        services.TryAddSingleton(sp => SubscriptionCatalog.Build(
            sp.GetServices<ModuleSubscriptions>(), sp.GetRequiredService<UseCaseCatalog>(), sp.GetRequiredService<IntegrationMessageCatalog>()));

        ValidationMessages.Configure(ValidatorOptions.Global);
        foreach (var assembly in validatorAssemblies)
        {
            services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, includeInternalTypes: true);
        }

        return services;
    }

    // 错误声明位于框架与各模块的 Domain 程序集（由 Application 按命名约定查找），也可位于模块的 Application 程序集中
    private static IEnumerable<Assembly> ErrorAssemblies(IReadOnlyCollection<Assembly> applicationAssemblies) =>
    [
        typeof(Kmic.BuildingBlocks.Domain.Results.Error).Assembly,
        .. applicationAssemblies,
        .. applicationAssemblies
            .SelectMany(a => a.GetReferencedAssemblies())
            .Where(name => name.Name is { } n && n.StartsWith("Kmic.", StringComparison.Ordinal) && n.EndsWith(".Domain", StringComparison.Ordinal))
            .Select(Assembly.Load),
    ];
}
