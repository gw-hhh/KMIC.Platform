using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Authorization;

/// <summary>
/// 授权诊断的参数
/// </summary>
/// <param name="UserId">用户 ID</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="ScopeObject">范围对象编码，不传时不返回数据范围</param>
public sealed record ExplainAuthorizationRequest(UserId UserId, RoleId RoleId, string? ScopeObject);
