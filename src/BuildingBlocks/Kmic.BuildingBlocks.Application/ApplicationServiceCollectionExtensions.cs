using System.Globalization;
using System.Reflection;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Application;

/// <summary>
/// 注册应用层框架服务
/// </summary>
/// <remarks>
/// 管道行为由宿主的 AddMediator 按顺序声明（Mediator 源码生成器要求在宿主中写成常量，原型 #1），这里只注册它们依赖的服务
/// </remarks>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// 注册执行上下文、发送入口、用例目录与校验器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="applicationAssemblies">各模块的 Application 程序集，用例目录与校验器从中扫描</param>
    public static IServiceCollection AddKmicApplication(this IServiceCollection services, IReadOnlyCollection<Assembly> applicationAssemblies)
    {
        ArgumentNullException.ThrowIfNull(applicationAssemblies);

        // 用例目录在注册时构建：声明有误时宿主启动即失败
        return services.AddKmicApplication(UseCaseCatalog.FromAssemblies(applicationAssemblies), applicationAssemblies);
    }

    // 测试只注册自己需要的消息：测试程序集里还有故意写错声明的样例
    internal static IServiceCollection AddKmicApplication(
        this IServiceCollection services,
        UseCaseCatalog catalog,
        IReadOnlyCollection<Assembly> validatorAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(catalog);
        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<PipelineOptions>();
        services.AddScoped<ExecutionScope>();
        services.AddScoped<IExecutionContext>(sp => sp.GetRequiredService<ExecutionScope>());
        services.AddSingleton<IExecutionScopeFactory, ExecutionScopeFactory>();
        services.AddScoped<ISender, Sender>();
        services.AddScoped<ICommandSender>(sp => sp.GetRequiredService<ISender>());
        services.AddScoped<IQuerySender>(sp => sp.GetRequiredService<ISender>());
        services.AddScoped<IContractQuerySender, ContractQuerySender>();

        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("zh-CN");
        foreach (var assembly in validatorAssemblies)
        {
            services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, includeInternalTypes: true);
        }

        return services;
    }
}
