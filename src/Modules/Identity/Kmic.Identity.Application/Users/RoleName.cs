namespace Kmic.Identity.Application.Users;

/// <summary>
/// 按角色 ID 批量取名字的结果
/// </summary>
/// <param name="Name">角色名称</param>
/// <param name="IsActive">角色是否启用</param>
/// <param name="IsProtected">是否受保护角色</param>
internal sealed record RoleName(string Name, bool IsActive, bool IsProtected);
