using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Grants.SetAnchors;

/// <summary>
/// 替换角色授予的组织锚点，空列表表示用用户所属组织
/// </summary>
/// <remarks>
/// 锚点是这个角色“本组织”“本组织及下级”范围的起点
/// </remarks>
/// <param name="Id">用户 ID</param>
/// <param name="ExpectedVersion">用户的预期版本号，不匹配时返回版本冲突</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="Anchors">新的锚点，必须是启用的组织单元</param>
[RequirePermission(IdentityPermissions.UserGrant)]
[Audited]
[Idempotent]
public sealed record SetGrantAnchorsCommand(UserId Id, long ExpectedVersion, RoleId RoleId, IReadOnlyList<OrgUnitId> Anchors) : ICommand<VersionedResponse<UserId>>;
