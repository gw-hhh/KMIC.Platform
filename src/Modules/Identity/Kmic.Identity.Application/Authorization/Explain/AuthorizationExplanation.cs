using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Authorization.Explain;

/// <summary>
/// 授权诊断结果
/// </summary>
/// <param name="UserId">用户 ID</param>
/// <param name="UserName">用户姓名</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="RoleName">角色名称</param>
/// <param name="RoleVersion">角色版本号，改了角色的权限或数据范围就会变</param>
/// <param name="Usable">用户现在能否以这个角色登录</param>
/// <param name="UnusableReason">不能登录的原因，能登录时为空</param>
/// <param name="Permissions">权限点编码，按编码排序；不能登录时为空集合</param>
/// <param name="DataScope">所查范围对象的数据范围；没有指定范围对象或不能登录时为空</param>
public sealed record AuthorizationExplanation(
    UserId UserId,
    string UserName,
    RoleId RoleId,
    string RoleName,
    long RoleVersion,
    bool Usable,
    string? UnusableReason,
    IReadOnlyList<string> Permissions,
    DataScopeExplanation? DataScope);
