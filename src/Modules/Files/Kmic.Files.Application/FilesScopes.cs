using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Files.Application;

/// <summary>
/// Files 模块的数据范围对象
/// </summary>
/// <remarks>
/// 每个有数据归属的聚合一个常量，写成“Files.资源”，并标 [DataScopeObject("显示名")]
/// </remarks>
[DataScopeObjectGroup("Files")]
public static class FilesScopes
{
}
