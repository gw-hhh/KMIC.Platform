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
/// 用运维账号的连接串直接连库，不经过命令管道，因为这两件事发生在系统还没有任何用户的时候或没有人能登录的时候。
/// 每个操作写一条安全事件，操作人记为系统账号，便于事后追溯
/// </remarks>
public sealed class IdentityBootstrapper
{
    private const string DefaultRootUnitName = "总部";

    private readonly PasswordService _passwords = new();
    private readonly PasswordPolicy _policy = new(Options.Create(new PasswordOptions()), new EmbeddedWeakPasswordList());

    /// <summary>
    /// 初始化：创建首个管理员、内置的系统管理员角色、授予，以及组织树的根单元
    /// </summary>
    /// <remarks>
    /// 只有系统里没有任何普通用户时才能执行，否则返回 <see cref="IdentityErrors.AlreadyBootstrapped" />
    /// </remarks>
    /// <param name="connectionString">运维账号的连接串</param>
    /// <param name="account">管理员账号</param>
    /// <param name="displayName">管理员姓名</param>
    /// <param name="password">管理员密码，要满足密码规则；用户首次登录后被要求修改</param>
    /// <param name="rootUnitName">组织树根单元的名称，不传用“总部”</param>
    /// <param name="now">当前时间</param>
    /// <param name="ct">取消令牌</param>
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
    /// 唯一的管理员忘了密码、又没有可用的手机号时的恢复路径。它的权限和直接改库相当，做成命令是为了留痕。
    /// 新密码要满足密码规则；账号带“必须修改密码”标记，下次登录先改密码
    /// </remarks>
    /// <param name="connectionString">运维账号的连接串</param>
    /// <param name="account">管理员账号</param>
    /// <param name="password">新密码</param>
    /// <param name="now">当前时间</param>
    /// <param name="ct">取消令牌</param>
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

        // 命令管道里这一步由领域事件的处理器完成；这里没有管道，直接作废会话并清零登录限制
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

    // 安全事件写入器只在请求与作业里可用；运维命令直接插一行，操作人是系统账号
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
