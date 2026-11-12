using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Roles.Catalogs;

/// <summary>
/// 查询数据范围对象目录：各模块声明的、可以单独设置范围等级的对象
/// </summary>
[RequirePermission(IdentityPermissions.RoleView)]
public sealed record ListDataScopeObjectsQuery : IQuery<IReadOnlyList<DataScopeObjectItem>>;
