using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Authorization;

/// <summary>
/// 授权诊断的参数
/// </summary>
/// <param name="UserId">用户 ID；必填</param>
/// <param name="RoleId">角色 ID；必填，可以是用户没有被授予的角色</param>
/// <param name="ScopeObject">范围对象编码，如 Sample.Specimen；不传时不返回数据范围</param>
public sealed record ExplainAuthorizationRequest(UserId UserId, RoleId RoleId, string? ScopeObject);
