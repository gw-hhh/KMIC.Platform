using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Users;

public sealed class UserKind : Enumeration<UserKind>
{
    public static readonly UserKind Normal = new("Normal", "普通用户");

    /// <summary>系统内置账号：不可登录、不可授予角色、不可修改。</summary>
    public static readonly UserKind System = new("System", "系统账号");

    private UserKind(string code, string name)
        : base(code, name)
    {
    }
}
