using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Application;
using Kmic.Identity.Application.SecurityEvents;
using Kmic.Identity.Contracts;
using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence;

/// <summary>
/// Identity 模块的查询上下文：不跟踪实体，查询处理器通过 IIdentityQueryDb 使用
/// </summary>
internal sealed class IdentityQueryDbContext(DbContextOptions<IdentityQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IIdentityQueryDb
{
    /// <inheritdoc />
    public override string Module => IdentityModuleInfo.Name;

    /// <inheritdoc />
    public IQueryable<User> Users => Set<User>();

    /// <inheritdoc />
    public IQueryable<Role> Roles => Set<Role>();

    /// <inheritdoc />
    public IQueryable<OrgTree> OrgTrees => Set<OrgTree>();

    /// <inheritdoc />
    public IQueryable<Session> Sessions => Set<Session>();

    /// <inheritdoc />
    public IQueryable<SecurityEventRow> SecurityEvents => Database.SqlQuery<SecurityEventRow>(
        $"""
        SELECT "Id", "OccurredAt", "Type", "Severity", "Reason", "UserId", "ActualUserId", "RoleId", "SessionId", "AttemptedAccount",
               host("ClientIp") AS "ClientIp", "ResourceType", "ResourceId", "Details"::text AS "Details", "TraceId"
        FROM "Identity"."SecurityEvent"
        """);

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInIdentityIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => IdentityModel.Configure(modelBuilder);
}
