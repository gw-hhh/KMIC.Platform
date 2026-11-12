using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Application.Users;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Impersonation.Start;

/// <summary>
/// 处理 <see cref="StartImpersonationCommand"/>
/// </summary>
public sealed class StartImpersonationCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IIdentityCommandReads db,
    IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
    IAllowedActions<UserTarget> allowedActions,
    IAccessTokenIssuer tokens,
    IOptions<SessionLifetimeOptions> sessionOptions,
    ISecurityEventWriter securityEvents,
    TimeProvider time,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<StartImpersonationCommand, AuthTokens>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<AuthTokens>> ExecuteAsync(StartImpersonationCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        // 在模拟会话里再开始模拟，会得到一条看不出真正操作人的链
        if (access.ImpersonatorUserId is not null)
        {
            return IdentityErrors.ImpersonationDenied;
        }

        var loaded = await UserAdminLoading.LoadForAsync(users, db, authorizer, command.TargetUserId, null, UserAdminAction.Impersonate, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var target = loaded.Value;
        if (!target.CanLogin)
        {
            return IdentityErrors.InvalidState;
        }

        var role = await ChooseRoleAsync(target, command.RoleId, cancellationToken);
        if (role is null)
        {
            return IdentityErrors.ImpersonationDenied.WithDetail(command.RoleId is null ? "他没有可用的角色" : "他没有这个角色，或这个角色已停用");
        }

        // 能不能写由“模拟身份时允许写入”权限点决定，开始时确定，会话期间不变
        var allowsWrite = allowedActions.For(await UserTargets.BuildAsync(db, target, access, cancellationToken)).Contains(UserAdminAction.ImpersonateWrite.Code, StringComparer.Ordinal);
        var now = time.GetUtcNow();
        var lifetime = sessionOptions.Value.ImpersonationLifetime;
        var session = Session.StartImpersonation(target.Id, role.Id, access.UserId, allowsWrite, command.Reason, ClientType.Web, Truncate(command.UserAgent, 200), command.ClientIp, now, lifetime);
        sessions.Add(session);

        securityEvents.Write(new SecurityEvent("ImpersonationStarted", SecuritySeverity.High, allowsWrite ? "Write" : "ReadOnly")
        {
            ResourceType = "User",
            ResourceId = target.Id.Value,
            Details = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["reason"] = Truncate(command.Reason, 200) ?? string.Empty,
                ["sessionId"] = session.Id.Value.ToString(),
                ["roleId"] = role.Id.Value.ToString(),
            },
        });

        // 没有刷新令牌，访问令牌的有效期就是会话的上限；撤销照样立即生效，因为每个请求都会查会话状态
        var accessToken = tokens.Issue(target.Id, session.Id, role.Id, ClientKinds.Claim(ClientType.Web), now, lifetime);
        return new AuthTokens(accessToken.Value, "Bearer", (int)(accessToken.ExpiresAt - now).TotalSeconds, null, PasswordChangeRequired: false);
    }

    // 指定了角色就必须是他拥有的、启用的；没指定时同登录：优先上次使用的，没有就取排在最前的启用角色
    private async Task<Role?> ChooseRoleAsync(User target, RoleId? requested, CancellationToken ct)
    {
        List<RoleId> granted = [.. target.Grants.Select(g => g.RoleId)];
        if (granted.Count == 0)
        {
            return null;
        }

        var active = await db.Roles.Where(r => granted.Contains(r.Id) && r.Status == RoleStatus.Active).OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToListAsync(ct);
        return requested is { } id ? active.Find(r => r.Id == id) : active.Find(r => r.Id == target.LastRoleId) ?? active.FirstOrDefault();
    }

    private static string? Truncate(string? text, int length) => text is null || text.Length <= length ? text : text[..length];
}
