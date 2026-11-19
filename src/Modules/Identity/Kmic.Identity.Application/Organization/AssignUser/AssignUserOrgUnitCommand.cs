using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Organization.AssignUser;

/// <summary>
/// 设置用户所属的组织单元
/// </summary>
/// <remarks>
/// 已有业务数据的所属组织不跟着改
/// </remarks>
/// <param name="Id">用户 ID</param>
/// <param name="ExpectedVersion">用户的预期版本号</param>
/// <param name="OrgUnitId">组织单元 ID</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record AssignUserOrgUnitCommand(UserId Id, long ExpectedVersion, OrgUnitId? OrgUnitId) : ICommand<VersionedResponse<UserId>>;
