using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Move;

/// <summary>
/// 把组织单元（连同它的下级）移到另一个单元之下
/// </summary>
/// <remarks>
/// 不能移到自己或自己的下级之下，根单元不能移动。用户调整组织后，他名下已有数据的所属组织不变
/// </remarks>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回版本冲突</param>
/// <param name="Id">要移动的组织单元</param>
/// <param name="NewParentId">新的上级，必须启用</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record MoveOrgUnitCommand(long ExpectedVersion, OrgUnitId Id, OrgUnitId NewParentId) : ICommand<VersionedResponse<OrgTreeId>>;
