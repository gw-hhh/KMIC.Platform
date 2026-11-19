using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Authorization.Explain;

/// <summary>
/// 授权诊断：用户在某个角色下有哪些权限，某个范围对象的数据范围怎么算出来
/// </summary>
/// <remarks>
/// 按后台作业的方式加载访问上下文，用户不用在线，结果和用户此刻登录后实际得到的一致
/// </remarks>
/// <param name="UserId">用户 ID</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="ScopeObject">范围对象编码，为空时不返回数据范围</param>
[RequirePermission(IdentityPermissions.AuthorizationExplain)]
[AuditedRead]
public sealed record ExplainAuthorizationQuery(UserId UserId, RoleId RoleId, string? ScopeObject) : IQuery<AuthorizationExplanation>;
