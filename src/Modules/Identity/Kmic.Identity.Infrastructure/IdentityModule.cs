using System.Reflection;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Application;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Application.Catalog;
using Kmic.Identity.Application.ContractQueries;
using Kmic.Identity.Application.Maintenance;
using Kmic.Identity.Contracts;
using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Kmic.Identity.Infrastructure.Access;
using Kmic.Identity.Infrastructure.Authentication;
using Kmic.Identity.Infrastructure.Authorization;
using Kmic.Identity.Infrastructure.Persistence;
using Kmic.Identity.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Identity.Infrastructure;

public sealed class IdentityModule : IModule
{
    public string Name => IdentityModuleInfo.Name;

    public Assembly ApplicationAssembly => typeof(IIdentityQueryDb).Assembly;

    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<IdentityDbContext, IdentityQueryDbContext, IIdentityQueryDb>();
        services.AddScoped<IIdentityCommandReads, IdentityCommandReads>();
        services.AddScoped<IPermissionCatalogStore, PermissionCatalogStore>();
        services.AddScoped<IStartupTask, SyncPermissionCatalogTask>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IOrgTreeRepository, OrgTreeRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        // Contracts 的实现：薄适配器，经 IContractQuerySender 调用本模块的契约查询
        services.AddScoped<IUserReferences, UserReferences>();
        services.AddScoped<IRoleReferences, RoleReferences>();
        services.AddScoped<IOrgUnitReferences, OrgUnitReferences>();
        services.AddScoped<IUserContacts, UserContacts>();

        // 供框架业务历史查询获取人员与角色显示名
        services.AddScoped<IActorReferences, ActorReferences>();

        // 加载器为单例，每次查询独立创建作用域，供请求和后台作业共用
        services.AddSingleton<IAccessContextLoader, AccessContextLoader>();

        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IWeakPasswordList, EmbeddedWeakPasswordList>();
        services.AddScoped<ILoginThrottleStore, LoginThrottleStore>();
        services.AddScoped<IOperatorRecheck, OperatorRecheck>();
        services.AddKmicOptions<LoginThrottleOptions>(LoginThrottleOptions.Section);
        services.AddKmicOptions<PasswordOptions>(PasswordOptions.Section);
        services.AddKmicOptions<SessionLifetimeOptions>(SessionLifetimeOptions.Section);
        services.AddKmicOptions<TrustedOriginsOptions>(TrustedOriginsOptions.Section);
        services.AddKmicOptions<CatalogOptions>(CatalogOptions.Section);
        services.AddSingleton<LoginThrottlePolicy>();
        services.AddSingleton<PasswordPolicy>();
        services.AddSingleton<TemporaryPasswordGenerator>();
    }

    public void ConfigureScheduling(ISchedulingBuilder scheduling)
    {
        ArgumentNullException.ThrowIfNull(scheduling);

        scheduling.Schedule("Identity.PurgeSessions", "0 30 3 * * ?", run => new PurgeSessionsCommand(run.ScheduledFireTime), o => o.DisplayName("清理已结束的会话"));
        scheduling.Schedule("Identity.PurgeRetiredPermissions", "0 0 5 ? * SUN", run => new PurgeRetiredPermissionsCommand(run.ScheduledFireTime), o => o.DisplayName("清理已废弃的权限点"));
        scheduling.Schedule("Identity.PurgeLoginThrottle", "0 0 4 * * ?", run => new PurgeLoginThrottleCommand(run.ScheduledFireTime), o => o.DisplayName("清理登录失败计数"));
    }
}
