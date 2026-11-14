using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Organization.AssignUser;

/// <summary>
/// 设置用户所属的组织单元
/// </summary>
/// <remarks>
/// 组织归属影响以用户所属组织为锚点的数据范围，因此单独授权和执行。
/// 调整用户归属不修改已有业务数据的所属组织
/// </remarks>
/// <param name="Id">用户 ID</param>
/// <param name="ExpectedVersion">用户的预期版本号，不匹配时返回版本冲突</param>
/// <param name="OrgUnitId">组织单元 ID，须启用；未指定时清空所属组织，依赖该组织的数据范围随之变化</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record AssignUserOrgUnitCommand(UserId Id, long ExpectedVersion, OrgUnitId? OrgUnitId) : ICommand<VersionedResponse<UserId>>;
