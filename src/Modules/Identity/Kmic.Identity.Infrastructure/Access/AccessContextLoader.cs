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
/// 加载访问上下文，会话和用户状态每次查库
/// </summary>
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
    public async Task<AccessLoadResult> LoadForRequestAsync(UserId user, SessionId session, RoleId role, CancellationToken ct) =>
        await queries.RunAsync<IdentityQueryDbContext, AccessLoadResult>(CallOrigin.Http, (db, token) => LoadRequestAsync(db, user, session, role, token), ct);

    /// <inheritdoc />
    public async Task<IAccessContext?> LoadForJobAsync(UserId user, RoleId role, CancellationToken ct) =>
        await queries.RunAsync<IdentityQueryDbContext, IAccessContext?>(CallOrigin.BackgroundJob, (db, token) => LoadJobAsync(db, user, role, token), ct);

    private async Task<AccessLoadResult> LoadRequestAsync(IdentityQueryDbContext db, UserId user, SessionId session, RoleId role, CancellationToken ct)
    {
        var state = await ReadSessionStateAsync(db, session, role, ct);

        // 令牌角色与会话当前角色不一致时拒绝请求，防止切换后继续使用旧角色
        if (state is not { SessionStatus: { } status, AbsoluteExpiresAt: { } expires, SessionUserId: { } sessionUser, CurrentRoleId: { } currentRole }
            || !string.Equals(status, SessionStatus.Active.Code, StringComparison.Ordinal)
            || time.GetUtcNow() >= expires
            || sessionUser != user.Value
            || currentRole != role.Value
            || !CanLogIn(state))
        {
            return AccessLoadResult.SessionInvalid();
        }

        var context = await BuildAsync(db, state, user, session, role, ct);
        return context.RoleAvailable ? AccessLoadResult.Loaded(context.Access) : AccessLoadResult.CurrentRoleUnavailable(context.Access);
    }

    private async Task<IAccessContext?> LoadJobAsync(IdentityQueryDbContext db, UserId user, RoleId role, CancellationToken ct)
    {
        var state = await ReadUserStateAsync(db, user, role, ct);
        if (state is null || !CanLogIn(state))
        {
            return null;
        }

        var context = await BuildAsync(db, state, user, _jobSession, role, ct);
        return context.RoleAvailable ? context.Access : null;
    }

    private static bool CanLogIn(AccessStateRow state) =>
        string.Equals(state.UserStatus, UserStatus.Active.Code, StringComparison.Ordinal)
        && string.Equals(state.UserKind, UserKind.Normal.Code, StringComparison.Ordinal);

    private async Task<(AccessContext Access, bool RoleAvailable)> BuildAsync(IdentityQueryDbContext db, AccessStateRow state, UserId user, SessionId session, RoleId role, CancellationToken ct)
    {
        var tree = await GetOrgTreeAsync(db, state.TreeVersion ?? 0, ct);
        var roleAvailable = state.GrantId is not null && string.Equals(state.RoleStatus, RoleStatus.Active.Code, StringComparison.Ordinal);
        if (!roleAvailable)
        {
            // 角色不可用时返回无角色权限的上下文，供已登录用户查看本人信息或切换角色
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
        var snapshot = await cache.GetOrCreateAsync(key, new RoleSnapshotLoader(db, permissions, role), static (loader, token) => loader.LoadAsync(token), _versionedEntry, cancellationToken: ct);
        if (snapshot.Version != version)
        {
            // 版本与缓存键不符时移除缓存，本次仍使用已加载的快照
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
        var snapshot = await cache.GetOrCreateAsync(key, new OrgTreeLoader(db), static (loader, token) => loader.LoadAsync(token), _versionedEntry, cancellationToken: ct);
        if (snapshot.Version != version)
        {
            await cache.RemoveAsync(key, ct);
        }

        return snapshot;
    }

    // 架构检查不允许加载状态公开可变成员，因此用私有加载器封装查询上下文
    private sealed class RoleSnapshotLoader(IdentityQueryDbContext db, PermissionCatalog catalog, RoleId roleId)
    {
        public async ValueTask<RoleSnapshot> LoadAsync(CancellationToken ct)
        {
            var role = await db.Set<Role>()
                .AsNoTracking()
                .Include(r => r.Permissions)
                .Include(r => r.DataScopes)
                .AsSplitQuery()
                .SingleOrDefaultAsync(r => r.Id == roleId, ct)
                ?? throw new InvalidOperationException($"授予指向不存在的角色 {roleId.Value}。");

            // 全权限角色使用当前目录的全部权限，其余角色过滤掉已退役权限
            var codes = role.GrantsAllPermissions
                ? catalog.All.Select(p => p.Code)
                : role.Permissions.Select(p => p.PermissionCode).Where(catalog.Contains);
            return new RoleSnapshot(
                role.ConcurrencyVersion,
                [.. codes.Distinct(StringComparer.Ordinal)],
                role.DefaultScopeLevel.ToScopeLevel(),
                role.DataScopes.ToDictionary(s => s.ScopeObject, s => s.Level.ToScopeLevel(), StringComparer.Ordinal));
        }
    }

    private sealed class OrgTreeLoader(IdentityQueryDbContext db)
    {
        public async ValueTask<OrgTreeSnapshot> LoadAsync(CancellationToken ct)
        {
            var tree = await db.Set<Domain.Organization.OrgTree>().AsNoTracking().Include(t => t.Units).AsSplitQuery().SingleOrDefaultAsync(ct);
            return tree is null
                ? OrgTreeSnapshot.Empty
                : new OrgTreeSnapshot(tree.ConcurrencyVersion, [.. tree.Units.Select(u => new OrgNode(u.Id.Value, u.ParentId?.Value))]);
        }
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
            SELECT NULL::uuid AS "SessionUserId", NULL::text AS "SessionStatus", NULL::timestamptz AS "AbsoluteExpiresAt", NULL::uuid AS "CurrentRoleId",
                   NULL::uuid AS "ImpersonatorUserId", false AS "ImpersonationAllowsWrite",
                   u."Status" AS "UserStatus", u."Kind" AS "UserKind", u."OrgUnitId", u."PasswordChangeRequired",
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
