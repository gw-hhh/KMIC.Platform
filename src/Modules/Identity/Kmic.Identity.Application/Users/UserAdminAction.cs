using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 用户管理操作，供资源策略和可用操作列表使用
/// </summary>
public sealed class UserAdminAction : Enumeration<UserAdminAction>
{
    /// <summary>
    /// 编辑用户资料；修改账号或手机号还须额外授权
    /// </summary>
    public static readonly UserAdminAction Edit = new("Edit", "编辑资料");

    /// <summary>
    /// 修改账号或手机号时的额外授权检查
    /// </summary>
    public static readonly UserAdminAction ChangeCredentials = new("ChangeCredentials", "修改账号或手机号");

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
    /// 开始模拟目标用户身份，默认只读
    /// </summary>
    public static readonly UserAdminAction Impersonate = new("Impersonate", "模拟身份");

    /// <summary>
    /// 允许模拟会话写入，须具备额外权限，创建会话时确定
    /// </summary>
    public static readonly UserAdminAction ImpersonateWrite = new("ImpersonateWrite", "模拟身份并允许修改数据");

    private UserAdminAction(string code, string name)
        : base(code, name)
    {
    }
}
