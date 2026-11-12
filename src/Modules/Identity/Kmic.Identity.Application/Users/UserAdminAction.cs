using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 管理用户时的动作：资源策略与详情里的“可用动作”都按它
/// </summary>
public sealed class UserAdminAction : Enumeration<UserAdminAction>
{
    /// <summary>
    /// 改姓名、工号、邮箱这些不涉及登录凭据的资料
    /// </summary>
    public static readonly UserAdminAction Edit = new("Edit", "编辑资料");

    /// <summary>
    /// 改账号、手机号：它们是登录凭据，对管理员的改动受保护
    /// </summary>
    public static readonly UserAdminAction ChangeCredentials = new("ChangeCredentials", "修改账号与手机号");

    /// <summary>
    /// 禁用
    /// </summary>
    public static readonly UserAdminAction Disable = new("Disable", "禁用");

    /// <summary>
    /// 启用
    /// </summary>
    public static readonly UserAdminAction Enable = new("Enable", "启用");

    /// <summary>
    /// 重置密码
    /// </summary>
    public static readonly UserAdminAction ResetPassword = new("ResetPassword", "重置密码");

    /// <summary>
    /// 解锁登录
    /// </summary>
    public static readonly UserAdminAction Unlock = new("Unlock", "解锁登录");

    /// <summary>
    /// 强制下线
    /// </summary>
    public static readonly UserAdminAction ForceLogout = new("ForceLogout", "强制下线");

    private UserAdminAction(string code, string name)
        : base(code, name)
    {
    }
}
