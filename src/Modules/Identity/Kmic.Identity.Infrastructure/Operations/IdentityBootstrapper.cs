using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;
using Kmic.Identity.Infrastructure.Authentication;
using Kmic.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Infrastructure.Operations;

/// <summary>
/// 运维命令的实现：首个管理员初始化、应急重置管理员密码
/// </summary>
/// <remarks>
/// 直接用运维连接执行，不经过命令管道
/// </remarks>
public sealed class IdentityBootstrapper
{
    private const string DefaultRootUnitName = "总部";

    private readonly PasswordService _passwords = new();
    private readonly PasswordPolicy _policy = new(Options.Create(new PasswordOptions()), new EmbeddedWeakPasswordList());

    /// <summary>
    /// 初始化首个管理员、系统管理员角色、角色授予及组织树根单元
    /// </summary>
    /// <remarks>
    /// 只能在没有普通用户时执行
    /// </remarks>
    public async Task<Result> BootstrapAsync(string connectionString, string account, string displayName, string password, string? rootUnitName, DateTimeOffset now, CancellationToken ct)
    {
        await using var db = StandaloneContexts.Create<IdentityDbContext>(connectionString);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        if (await db.Set<User>().AnyAsync(u => u.Kind == UserKind.Normal, ct))
        {
            return IdentityErrors.AlreadyBootstrapped;
        }

        var created = User.Create(account, displayName, null, null, null, null, "初始化");
        if (created.IsFailure)
        {
            return created.Error;
        }

        var admin = created.Value;
        var allowed = _policy.Validate(password, admin.Account, admin.Mobile);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        var tree = OrgTree.Create(string.IsNullOrWhiteSpace(rootUnitName) ? DefaultRootUnitName : rootUnitName);
        if (tree.IsFailure)
        {
            return tree.Error;
        }

        var role = Role.CreateSystemAdministrator();
        admin.SetPassword(_passwords.Hash(admin, password), mustChange: true, byAdministrator: false, now);
        admin.Grant(role.Id, UserId.System, now, []);
        db.Add(role);
        db.Add(admin);
        db.Add(tree.Value);
        await db.SaveForToolAsync(ct);
        await WriteEventAsync(db, "BootstrapExecuted", "High", admin.Id, admin.Account, now, ct);
        await transaction.CommitAsync(ct);
        return Result.Success();
    }

    /// <summary>
    /// 应急重置：为持有受保护角色的账号设置新密码，作废它的全部会话，清零登录限制
    /// </summary>
    /// <remarks>
    /// 不会启用已禁用的账号
    /// </remarks>
    public async Task<Result> ResetAdministratorPasswordAsync(string connectionString, string account, string password, DateTimeOffset now, CancellationToken ct)
    {
        await using var db = StandaloneContexts.Create<IdentityDbContext>(connectionString);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var user = await db.Set<User>().Include(u => u.Grants).AsSplitQuery().SingleOrDefaultAsync(u => u.Account == account.Trim(), ct);
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var roleIds = user.Grants.Select(g => g.RoleId).ToList();
        if (!await db.Set<Role>().AnyAsync(r => roleIds.Contains(r.Id) && r.IsProtected, ct))
        {
            return IdentityErrors.NotProtectedHolder;
        }

        var allowed = _policy.Validate(password, user.Account, user.Mobile);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        var set = user.SetPassword(_passwords.Hash(user, password), mustChange: true, byAdministrator: true, now);
        if (set.IsFailure)
        {
            return set;
        }

        await db.SaveForToolAsync(ct);

        // 命令管道通过领域事件作废会话；应急重置直接作废会话，并额外清零登录限制
        var loginHash = LoginNames.Hash(user.Account);
        await db.Database.ExecuteSqlAsync(
            $"""
            UPDATE "Identity"."Session"
            SET "Status" = 'Revoked', "RevokedReason" = 'PasswordReset', "RevokedAt" = {now}, "ConcurrencyVersion" = "ConcurrencyVersion" + 1
            WHERE ("UserId" = {user.Id.Value} OR "ImpersonatorUserId" = {user.Id.Value}) AND "Status" = 'Active'
            """,
            ct);
        await db.Database.ExecuteSqlAsync($"""DELETE FROM "Identity"."LoginThrottle" WHERE "LoginNameHash" = {loginHash}""", ct);
        await WriteEventAsync(db, "PasswordReset", "High", user.Id, user.Account, now, ct);
        await transaction.CommitAsync(ct);
        return Result.Success();
    }

    private static async Task WriteEventAsync(IdentityDbContext db, string type, string severity, UserId target, string account, DateTimeOffset now, CancellationToken ct)
    {
        var severityValue = string.Equals(severity, "High", StringComparison.Ordinal) ? (short)2 : (short)0;
        await db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO "Identity"."SecurityEvent" ("Id", "OccurredAt", "Type", "Severity", "Reason", "UserId", "ActualUserId", "AttemptedAccount", "TraceId")
            VALUES ({Guid.CreateVersion7()}, {now}, {type}, {severityValue}, {"Operator"}, {target.Value}, {UserId.System.Value}, {account}, {new string('0', 32)})
            """,
            ct);
    }
}
