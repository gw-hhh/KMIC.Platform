namespace Kmic.Identity.Application.Users;

/// <summary>
/// 角色名称及启用、保护标记
/// </summary>
/// <param name="Name">角色名称</param>
/// <param name="IsActive">角色是否启用</param>
/// <param name="IsProtected">是否受保护角色</param>
internal sealed record RoleName(string Name, bool IsActive, bool IsProtected);
