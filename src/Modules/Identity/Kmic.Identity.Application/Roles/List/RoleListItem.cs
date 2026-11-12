using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.List;

/// <summary>
/// 角色列表的一行
/// </summary>
/// <param name="Id">角色 ID</param>
/// <param name="Name">名称</param>
/// <param name="Description">说明</param>
/// <param name="Status">状态</param>
/// <param name="IsProtected">是否受保护角色</param>
/// <param name="IsBuiltIn">是否内置（拥有全部权限点）</param>
/// <param name="SortOrder">次序</param>
/// <param name="HolderCount">持有人数</param>
/// <param name="ConcurrencyVersion">版本号，输出为 version</param>
public sealed record RoleListItem(
    RoleId Id,
    string Name,
    string? Description,
    RoleStatus Status,
    bool IsProtected,
    bool IsBuiltIn,
    int SortOrder,
    int HolderCount,
    [property: JsonPropertyName("version")] long ConcurrencyVersion);
