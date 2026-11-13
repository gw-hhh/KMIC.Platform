using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Files.Application;

/// <summary>
/// Files 模块的权限点
/// </summary>
/// <remarks>
/// 每个权限点是一个常量，写成“Files.资源.动作”，并标 [Permission("显示名")]
/// </remarks>
[PermissionGroup("Files", "文件")]
public static class FilesPermissions
{
}
