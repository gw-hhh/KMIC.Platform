using System.Reflection;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Contracts;
using Kmic.Operations.Application;
using Kmic.Operations.Application.Audit.Record;
using Kmic.Operations.Contracts;
using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Jobs;
using Kmic.Operations.Infrastructure.Persistence;
using Kmic.Operations.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Operations.Infrastructure;

/// <summary>
/// Operations 模块的服务端注册类
/// </summary>
/// <remarks>
/// 处理器、校验器、EF 配置自动发现；这里只写自动发现做不到的注册
/// </remarks>
public sealed class OperationsModule : IModule
{
    /// <inheritdoc />
    public string Name => OperationsModuleInfo.Name;

    /// <inheritdoc />
    public Assembly ApplicationAssembly => typeof(IOperationsQueryDb).Assembly;

    /// <inheritdoc />
    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<OperationsDbContext, OperationsQueryDbContext, IOperationsQueryDb>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IScheduledJobSettingRepository, ScheduledJobSettingRepository>();
    }

    /// <inheritdoc />
    public void ConfigureMessaging(IMessagingBuilder messaging)
    {
        ArgumentNullException.ThrowIfNull(messaging);

        // 订阅名存进收件箱，发布后不能改
        messaging.Subscribe<AuditRecorded, RecordAuditCommand>("Operations.RecordAudit", (message, _) => new RecordAuditCommand(message));
    }

    /// <inheritdoc />
    public void DeclareDependencies(IModuleDependencies dependencies)
    {
        ArgumentNullException.ThrowIfNull(dependencies);

        dependencies.DependsOn(IdentityModuleInfo.Name);
    }
}
