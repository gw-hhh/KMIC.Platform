using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Grants.SetAnchors;

/// <summary>
/// 整体替换一个授予上的锚点
/// </summary>
/// <remarks>
/// 锚点决定这个角色的“本组织”“本组织及下级”从哪些组织单元算起；不给锚点就从用户所属的组织单元算。
/// 规则同授予：不能改自己的，受保护角色的要求当前角色也是受保护角色
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="ExpectedVersion">调用方看到的用户版本号，别人改过就返回版本冲突</param>
/// <param name="RoleId">角色</param>
/// <param name="Anchors">新的锚点，必须是启用的组织单元</param>
[RequirePermission(IdentityPermissions.UserGrant)]
[Audited]
[Idempotent]
public sealed record SetGrantAnchorsCommand(UserId Id, long ExpectedVersion, RoleId RoleId, IReadOnlyList<OrgUnitId> Anchors) : ICommand<VersionedResponse<UserId>>;
