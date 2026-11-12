using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 用户管理命令共用的加载与授权检查
/// </summary>
internal static class UserAdminLoading
{
    /// <summary>
    /// 加载要修改的用户（传了版本号时检查版本），并按资源策略检查操作者能否做这个动作
    /// </summary>
    public static async Task<Result<User>> LoadForAsync(
        IUserRepository users,
        IIdentityQueryDb db,
        IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
        UserId id,
        long? expectedVersion,
        UserAdminAction action,
        IAccessContext access,
        CancellationToken ct)
    {
        // 禁用要判断“是不是最后一个管理员”，先排队再读，两个并发的禁用才不会各自看到对方还启用着
        if (action == UserAdminAction.Disable)
        {
            await users.LockProtectedHolderChecksAsync(ct);
        }

        var loaded = await users.GetForUpdateAsync(id, expectedVersion, ct);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var allowed = authorizer.Check(await UserTargets.BuildAsync(db, loaded.Value, access, ct), action);
        return allowed.IsFailure ? allowed.Error : loaded.Value;
    }
}
