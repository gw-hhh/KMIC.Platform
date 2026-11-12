using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Organization.AssignUser;

/// <summary>
/// 设置用户所属的组织单元
/// </summary>
/// <remarks>
/// 单独成命令，因为它直接改变用户的数据范围：“本组织”“本组织及下级”都从这里算起。
/// 调整之后，他名下已有数据的所属组织不变，那是创建数据时的业务事实
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="ExpectedVersion">调用方看到的用户版本号，别人改过就返回版本冲突</param>
/// <param name="OrgUnitId">组织单元，必须启用；不给表示不属于任何单元，数据范围只剩“本人”</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record AssignUserOrgUnitCommand(UserId Id, long ExpectedVersion, OrgUnitId? OrgUnitId) : ICommand<VersionedResponse<UserId>>;
