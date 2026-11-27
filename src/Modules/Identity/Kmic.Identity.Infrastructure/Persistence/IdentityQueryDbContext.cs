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

internal sealed class IdentityQueryDbContext(DbContextOptions<IdentityQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IIdentityQueryDb
{
    public override string Module => IdentityModuleInfo.Name;

    public IQueryable<User> Users => Set<User>();

    public IQueryable<Role> Roles => Set<Role>();

    public IQueryable<OrgTree> OrgTrees => Set<OrgTree>();

    public IQueryable<Session> Sessions => Set<Session>();

    public IQueryable<SecurityEventRow> SecurityEvents => Database.SqlQuery<SecurityEventRow>(
        $"""
        SELECT "Id", "OccurredAt", "Type", "Severity", "Reason", "UserId", "ActualUserId", "RoleId", "SessionId", "AttemptedAccount",
               host("ClientIp") AS "ClientIp", "ResourceType", "ResourceId", "Details"::text AS "Details", "TraceId"
        FROM "Identity"."SecurityEvent"
        """);

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInIdentityIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => IdentityModel.Configure(modelBuilder);
}
