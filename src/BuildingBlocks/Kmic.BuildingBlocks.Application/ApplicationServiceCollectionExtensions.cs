using System.Globalization;
using System.Reflection;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Dictionaries;
using Kmic.BuildingBlocks.Application.Errors;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
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

/// <summary>
/// 应用层服务注册
/// </summary>
/// <remarks>
/// 管道行为在宿主的 AddMediator 里按顺序声明，Mediator 源码生成器要求这样写。这里只注册它们依赖的服务
/// </remarks>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// 注册应用层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="applicationAssemblies">各模块的 Application 程序集</param>
    public static IServiceCollection AddKmicApplication(this IServiceCollection services, IReadOnlyCollection<Assembly> applicationAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(applicationAssemblies);

        // 各目录在注册时互相核对，声明有误时宿主直接启动失败
        var useCases = UseCaseCatalog.FromAssemblies(applicationAssemblies);
        var permissions = PermissionCatalog.FromAssemblies(applicationAssemblies);
        var settings = SystemSettingCatalog.FromAssemblies(applicationAssemblies);
        var types = applicationAssemblies.Distinct().SelectMany(a => a.GetTypes()).ToList();
        var resourceActions = services.AddResourceAuthorization(types);
        services.AddDomainEventHandlers(types);
        var jobs = BackgroundJobCatalog.FromAssemblies(applicationAssemblies);
        permissions.Verify(useCases, [.. resourceActions, .. settings.PermissionReferences, .. jobs.PermissionReferences]);

        services.AddSingleton(jobs);
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
        services.AddOptions<CursorOptions>();
        services.AddSingleton<ICursorProtector, HmacCursorProtector>();
        return services.AddKmicApplication(useCases, applicationAssemblies);
    }

    /// <summary>
    /// 登记一个模块的订阅
    /// </summary>
    /// <remarks>
    /// 校验在第一次取订阅目录时统一做，宿主启动时要取一次
    /// </remarks>
    /// <param name="services">服务集合</param>
    /// <param name="module">订阅方模块名</param>
    /// <param name="configure">订阅声明</param>
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

    /// <summary>
    /// 登记一个模块的定时任务
    /// </summary>
    /// <remarks>
    /// 校验在第一次取定时任务目录时统一做，启动时同步到调度器那一步会取
    /// </remarks>
    /// <param name="services">服务集合</param>
    /// <param name="module">模块名</param>
    /// <param name="configure">定时任务声明</param>
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

    // 测试程序集里有故意写错声明的样例，所以测试只注册自己用到的消息
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
        services.AddScoped<IntegrationEventPublisher>();
        services.AddScoped<IIntegrationEventPublisher>(sp => sp.GetRequiredService<IntegrationEventPublisher>());
        services.TryAddSingleton(sp => SubscriptionCatalog.Build(
            sp.GetServices<ModuleSubscriptions>(), sp.GetRequiredService<UseCaseCatalog>(), sp.GetRequiredService<IntegrationMessageCatalog>()));

        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("zh-CN");
        foreach (var assembly in validatorAssemblies)
        {
            services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, includeInternalTypes: true);
        }

        return services;
    }

    // 错误声明在框架的 Domain 程序集和各模块的 Domain 程序集里（Application 引用着它们，按命名约定找），模块的 Application 里也可以有
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
