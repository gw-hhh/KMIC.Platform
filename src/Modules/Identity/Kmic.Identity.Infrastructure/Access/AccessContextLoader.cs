using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Kmic.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 访问上下文加载器：每个请求一次状态查询，角色与组织树的内容按版本缓存
/// </summary>
/// <remarks>
/// 状态查询每次都查库：会话是否有效、用户是否启用、授予是否还在、角色是否启用，这些变化要在下一个请求就生效，不能缓存。
/// 角色授权快照与组织树快照的键里带版本号，内容不可变，所以不需要失效。
/// 快照里的版本号是权威的：状态查询之后、读取快照之前角色又被修改时，读到的是更新的版本，本次请求用快照自己的版本，
/// 内容与版本始终一致，并且不让错位的内容留在缓存里
/// </remarks>
internal sealed class AccessContextLoader(IQueryScopeFactory queries, HybridCache cache, PermissionCatalog permissions, TimeProvider time) : IAccessContextLoader
{
    // 后台作业没有会话，访问上下文里的会话用这个固定值
    private static readonly SessionId _jobSession = SessionId.From(new Guid("00000000-0000-0000-0000-0000000000a1"));

    private static readonly HybridCacheEntryOptions _versionedEntry = new()
    {
        LocalCacheExpiration = TimeSpan.FromHours(1),
        Expiration = TimeSpan.FromDays(1),
    };

    private static readonly DataScopeGrant _selfOnly = new(ScopeLevel.Self, FrozenDictionary<string, ScopeLevel>.Empty, [], FrozenDictionary<string, IReadOnlyList<Guid>>.Empty);

    /// <inheritdoc />
    /// <inheritdoc />
    public async Task<AccessLoadResult> LoadForRequestAsync(UserId user, SessionId session, RoleId role, CancellationToken ct) =>
        await queries.RunAsync<IdentityQueryDbContext, AccessLoadResult>(CallOrigin.Http, (db, token) => LoadRequestAsync(db, user, session, role, token), ct);

    /// <inheritdoc />
    public async Task<IAccessContext?> LoadForJobAsync(UserId user, RoleId role, CancellationToken ct) =>
        await queries.RunAsync<IdentityQueryDbContext, IAccessContext?>(CallOrigin.BackgroundJob, (db, token) => LoadJobAsync(db, user, role, token), ct);

    private async Task<AccessLoadResult> LoadRequestAsync(IdentityQueryDbContext db, UserId user, SessionId session, RoleId role, CancellationToken ct)
    {
        var state = await ReadSessionStateAsync(db, session, role, ct);

        // 访问令牌里的角色和会话当前的角色不一致时以会话为准：切换角色之后旧令牌不能继续用旧角色操作
        if (state is not { SessionStatus: { } status, AbsoluteExpiresAt: { } expires, SessionUserId: { } sessionUser, CurrentRoleId: { } currentRole }
            || !string.Equals(status, SessionStatus.Active.Code, StringComparison.Ordinal)
            || time.GetUtcNow() >= expires
            || sessionUser != user.Value
            || currentRole != role.Value
            || !IsLoginable(state))
        {
            return AccessLoadResult.SessionInvalid();
        }

        var context = await BuildAsync(db, state, user, session, role, ct);
        return context.RoleAvailable ? AccessLoadResult.Loaded(context.Access) : AccessLoadResult.CurrentRoleUnavailable(context.Access);
    }

    private async Task<IAccessContext?> LoadJobAsync(IdentityQueryDbContext db, UserId user, RoleId role, CancellationToken ct)
    {
        var state = await ReadUserStateAsync(db, user, role, ct);
        if (state is null || !IsLoginable(state))
        {
            return null;
        }

        var context = await BuildAsync(db, state, user, _jobSession, role, ct);
        return context.RoleAvailable ? context.Access : null;
    }

    private static bool IsLoginable(AccessStateRow state) =>
        string.Equals(state.UserStatus, UserStatus.Active.Code, StringComparison.Ordinal)
        && string.Equals(state.UserKind, UserKind.Normal.Code, StringComparison.Ordinal);

    private async Task<(AccessContext Access, bool RoleAvailable)> BuildAsync(IdentityQueryDbContext db, AccessStateRow state, UserId user, SessionId session, RoleId role, CancellationToken ct)
    {
        var tree = await GetOrgTreeAsync(db, state.TreeVersion ?? 0, ct);
        var roleAvailable = state.GrantId is not null && string.Equals(state.RoleStatus, RoleStatus.Active.Code, StringComparison.Ordinal);
        if (!roleAvailable)
        {
            // 用户还在、会话也有效，只是这个角色不能再用：只给一个没有任何角色权限的上下文，够查看自己和切换角色
            return (Create(state, user, session, role, authorizationVersion: 0, permissions: FrozenSet<string>.Empty, _selfOnly, OrgTreeSnapshot.Empty), false);
        }

        var snapshot = await GetRoleSnapshotAsync(db, role, state.RoleVersion ?? 0, ct);
        var overrides = snapshot.ScopeOverrides.ToFrozenDictionary(o => o.Key, o => (ScopeLevel)o.Value, StringComparer.Ordinal);
        var grant = new DataScopeGrant(
            (ScopeLevel)snapshot.DefaultScopeLevel,
            overrides,
            [.. state.AnchorIds.Select(OrgUnitId.From)],
            FrozenDictionary<string, IReadOnlyList<Guid>>.Empty);
        return (Create(state, user, session, role, snapshot.Version, snapshot.Permissions, grant, tree), true);
    }

    private static AccessContext Create(
        AccessStateRow state,
        UserId user,
        SessionId session,
        RoleId role,
        long authorizationVersion,
        IReadOnlySet<string> permissions,
        DataScopeGrant grant,
        IOrgHierarchy hierarchy) =>
        new()
        {
            UserId = user,
            SessionId = session,
            RoleId = role,
            AuthorizationVersion = authorizationVersion,
            Permissions = permissions,
            OrgUnitId = state.OrgUnitId is { } unit ? OrgUnitId.From(unit) : null,
            DataScopeGrant = grant,
            OrgHierarchy = hierarchy,
            ImpersonatorUserId = state.ImpersonatorUserId is { } impersonator ? UserId.From(impersonator) : null,
            ImpersonationAllowsWrite = state.ImpersonationAllowsWrite,
            PasswordChangeRequired = state.PasswordChangeRequired,
        };

    private async Task<RoleSnapshot> GetRoleSnapshotAsync(IdentityQueryDbContext db, RoleId role, long version, CancellationToken ct)
    {
        var key = IdentityCacheKeys.RoleSnapshot(role, version);
        var snapshot = await cache.GetOrCreateAsync(key, (db, permissions, role), LoadRoleAsync, _versionedEntry, cancellationToken: ct);
        if (snapshot.Version != version)
        {
            // 键里的版本比内容旧：不让错位的内容留在缓存里，本次请求用快照自己的版本
            await cache.RemoveAsync(key, ct);
        }

        return snapshot;
    }

    private async Task<OrgTreeSnapshot> GetOrgTreeAsync(IdentityQueryDbContext db, long version, CancellationToken ct)
    {
        if (version == 0)
        {
            return OrgTreeSnapshot.Empty;
        }

        var key = IdentityCacheKeys.OrgTree(version);
        var snapshot = await cache.GetOrCreateAsync(key, db, LoadOrgTreeAsync, _versionedEntry, cancellationToken: ct);
        if (snapshot.Version != version)
        {
            await cache.RemoveAsync(key, ct);
        }

        return snapshot;
    }

    private static async ValueTask<RoleSnapshot> LoadRoleAsync((IdentityQueryDbContext Db, PermissionCatalog Catalog, RoleId Role) state, CancellationToken ct)
    {
        var role = await state.Db.Set<Role>()
            .AsNoTracking()
            .Include(r => r.Permissions)
            .Include(r => r.DataScopes)
            .AsSplitQuery()
            .SingleOrDefaultAsync(r => r.Id == state.Role, ct)
            ?? throw new InvalidOperationException($"角色 {state.Role.Value} 不存在，授予指向了一个不存在的角色。");

        // 全权限角色不存权限点行，拥有当前目录里的全部；其余角色只保留目录里还有的，已退役的权限点没有代码会检查它
        var codes = role.GrantsAllPermissions
            ? state.Catalog.All.Select(p => p.Code)
            : role.Permissions.Select(p => p.PermissionCode).Where(state.Catalog.Contains);
        return new RoleSnapshot(
            role.ConcurrencyVersion,
            codes.ToFrozenSet(StringComparer.Ordinal),
            (int)role.DefaultScopeLevel,
            role.DataScopes.ToFrozenDictionary(s => s.ScopeObject, s => (int)s.Level, StringComparer.Ordinal));
    }

    private static async ValueTask<OrgTreeSnapshot> LoadOrgTreeAsync(IdentityQueryDbContext db, CancellationToken ct)
    {
        var tree = await db.Set<Domain.Organization.OrgTree>().AsNoTracking().Include(t => t.Units).AsSplitQuery().SingleOrDefaultAsync(ct);
        return tree is null
            ? OrgTreeSnapshot.Empty
            : new OrgTreeSnapshot(tree.ConcurrencyVersion, [.. tree.Units.Select(u => new OrgNode(u.Id.Value, u.ParentId?.Value))]);
    }

    private static async Task<AccessStateRow?> ReadSessionStateAsync(IdentityQueryDbContext db, SessionId session, RoleId role, CancellationToken ct) =>
        await db.Database.SqlQuery<AccessStateRow>(
            $"""
            SELECT s."UserId" AS "SessionUserId", s."Status" AS "SessionStatus", s."AbsoluteExpiresAt", s."CurrentRoleId",
                   s."ImpersonatorUserId", s."ImpersonationAllowsWrite",
                   u."Status" AS "UserStatus", u."Kind" AS "UserKind", u."OrgUnitId", u."PasswordChangeRequired",
                   g."Id" AS "GrantId", r."Status" AS "RoleStatus", r."ConcurrencyVersion" AS "RoleVersion",
                   COALESCE(a."Ids", ARRAY[]::uuid[]) AS "AnchorIds",
                   (SELECT t."ConcurrencyVersion" FROM "Identity"."OrgTree" t LIMIT 1) AS "TreeVersion"
            FROM "Identity"."Session" s
            JOIN "Identity"."User" u ON u."Id" = s."UserId"
            LEFT JOIN "Identity"."UserRoleGrant" g ON g."UserId" = u."Id" AND g."RoleId" = {role.Value}
            LEFT JOIN "Identity"."Role" r ON r."Id" = g."RoleId"
            LEFT JOIN LATERAL (SELECT array_agg(ga."OrgUnitId") AS "Ids" FROM "Identity"."GrantAnchor" ga WHERE ga."GrantId" = g."Id") a ON true
            WHERE s."Id" = {session.Value}
            """).SingleOrDefaultAsync(ct);

    private static async Task<AccessStateRow?> ReadUserStateAsync(IdentityQueryDbContext db, UserId user, RoleId role, CancellationToken ct) =>
        await db.Database.SqlQuery<AccessStateRow>(
            $"""
            SELECT u."Status" AS "UserStatus", u."Kind" AS "UserKind", u."OrgUnitId", u."PasswordChangeRequired",
                   g."Id" AS "GrantId", r."Status" AS "RoleStatus", r."ConcurrencyVersion" AS "RoleVersion",
                   COALESCE(a."Ids", ARRAY[]::uuid[]) AS "AnchorIds",
                   (SELECT t."ConcurrencyVersion" FROM "Identity"."OrgTree" t LIMIT 1) AS "TreeVersion"
            FROM "Identity"."User" u
            LEFT JOIN "Identity"."UserRoleGrant" g ON g."UserId" = u."Id" AND g."RoleId" = {role.Value}
            LEFT JOIN "Identity"."Role" r ON r."Id" = g."RoleId"
            LEFT JOIN LATERAL (SELECT array_agg(ga."OrgUnitId") AS "Ids" FROM "Identity"."GrantAnchor" ga WHERE ga."GrantId" = g."Id") a ON true
            WHERE u."Id" = {user.Value}
            """).SingleOrDefaultAsync(ct);
}
