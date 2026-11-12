using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户的种类
/// </summary>
public sealed class UserKind : Enumeration<UserKind>
{
    /// <summary>
    /// 能登录系统的人
    /// </summary>
    public static readonly UserKind Normal = new("Normal", "普通用户");

    /// <summary>
    /// 代表“系统”的账号：不能登录、不能授予角色、不能修改
    /// </summary>
    public static readonly UserKind System = new("System", "系统账号");

    private UserKind(string code, string name)
        : base(code, name)
    {
    }
}
