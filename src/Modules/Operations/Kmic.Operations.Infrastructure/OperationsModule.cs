using System.Reflection;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Contracts;
using Kmic.Identity.Contracts;
using Kmic.Operations.Application;
using Kmic.Operations.Application.Audit.Export;
using Kmic.Operations.Application.Audit.Record;
using Kmic.Operations.Application.ClientErrors;
using Kmic.Operations.Contracts;
using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Jobs;
using Kmic.Operations.Infrastructure.Capacity;
using Kmic.Operations.Infrastructure.ClientErrors;
using Kmic.Operations.Infrastructure.Persistence;
using Kmic.Operations.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Operations.Infrastructure;

public sealed class OperationsModule : IModule
{
    public string Name => OperationsModuleInfo.Name;

    public Assembly ApplicationAssembly => typeof(IOperationsQueryDb).Assembly;

    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<OperationsDbContext, OperationsQueryDbContext, IOperationsQueryDb>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IScheduledJobSettingRepository, ScheduledJobSettingRepository>();
        services.AddKmicOptions<ClientErrorOptions>(ClientErrorOptions.Section);
        services.AddKmicOptions<AuditExportOptions>(AuditExportOptions.Section);
        services.AddSingleton<IClientErrorLimiter, ClientErrorLimiter>();
        services.AddSingleton<IClientErrorLog, ClientErrorLog>();
        services.AddScoped<IMessagingCapacitySource, MessagingCapacitySource>();
    }

    public void ConfigureMessaging(IMessagingBuilder messaging)
    {
        ArgumentNullException.ThrowIfNull(messaging);

        // 订阅名会写入收件箱，发布后不可变更
        messaging.Subscribe<AuditRecorded, RecordAuditCommand>("Operations.RecordAudit", (message, _) => new RecordAuditCommand(message));
    }

    public void DeclareDependencies(IModuleDependencies dependencies)
    {
        ArgumentNullException.ThrowIfNull(dependencies);

        dependencies.DependsOn(IdentityModuleInfo.Name);
        dependencies.DependsOn(FilesModuleInfo.Name);
    }
}
