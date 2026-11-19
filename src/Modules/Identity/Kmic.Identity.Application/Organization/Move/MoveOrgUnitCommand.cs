using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Move;

/// <summary>
/// 移动组织单元及其下级
/// </summary>
/// <remarks>
/// 已有业务数据的所属组织不跟着改
/// </remarks>
/// <param name="ExpectedVersion">组织树的预期版本号</param>
/// <param name="Id">要移动的组织单元</param>
/// <param name="NewParentId">新的上级</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record MoveOrgUnitCommand(long ExpectedVersion, OrgUnitId Id, OrgUnitId NewParentId) : ICommand<VersionedResponse<OrgTreeId>>;
