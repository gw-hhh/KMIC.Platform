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

    /// <summary>
    /// 以只读方式模拟他的身份
    /// </summary>
    public static readonly UserAdminAction Impersonate = new("Impersonate", "模拟身份");

    /// <summary>
    /// 以可以修改数据的方式模拟他的身份：需要另外的权限点，开始模拟时按它决定会话是否可写
    /// </summary>
    public static readonly UserAdminAction ImpersonateWrite = new("ImpersonateWrite", "模拟身份并允许修改数据");

    private UserAdminAction(string code, string name)
        : base(code, name)
    {
    }
}
