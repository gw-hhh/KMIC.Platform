using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.Get;

/// <summary>
/// 角色详情
/// </summary>
/// <param name="Id">角色 ID</param>
/// <param name="ConcurrencyVersion">版本号，输出为 version，也是授权版本号</param>
/// <param name="Name">名称</param>
/// <param name="Description">说明</param>
/// <param name="Status">状态</param>
/// <param name="IsProtected">是否受保护角色</param>
/// <param name="IsBuiltIn">是否内置（拥有全部权限点，权限点与数据范围不能手工改）</param>
/// <param name="SortOrder">次序</param>
/// <param name="DefaultScopeLevel">默认数据范围等级</param>
/// <param name="Permissions">拥有的权限点；内置角色拥有目录里的全部，这里为空</param>
/// <param name="DataScopes">按范围对象单独设置的等级</param>
/// <param name="HolderCount">持有人数</param>
/// <param name="AllowedActions">当前用户现在可以对它执行的动作</param>
public sealed record RoleDetailResponse(
    RoleId Id,
    [property: JsonPropertyName("version")] long ConcurrencyVersion,
    string Name,
    string? Description,
    RoleStatus Status,
    bool IsProtected,
    bool IsBuiltIn,
    int SortOrder,
    RoleScopeLevel DefaultScopeLevel,
    IReadOnlyList<RolePermissionItem> Permissions,
    IReadOnlyList<RoleDataScopeItem> DataScopes,
    int HolderCount,
    IReadOnlyList<string> AllowedActions);
