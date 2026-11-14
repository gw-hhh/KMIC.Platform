using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Roles.Catalogs;

/// <summary>
/// 查询可单独配置等级的数据范围对象
/// </summary>
[RequirePermission(IdentityPermissions.RoleView)]
public sealed record ListDataScopeObjectsQuery : IQuery<IReadOnlyList<DataScopeObjectItem>>;
