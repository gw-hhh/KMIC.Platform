using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application;

/// <summary>
/// Operations 模块的数据范围对象
/// </summary>
/// <remarks>
/// 每个有数据归属的聚合一个常量，写成“Operations.资源”，并标 [DataScopeObject("显示名")]
/// </remarks>
[DataScopeObjectGroup("Operations")]
public static class OperationsScopes
{
}
