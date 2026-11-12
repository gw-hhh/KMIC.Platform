using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Organization.Create;

/// <summary>
/// 在某个启用的组织单元下添加下级
/// </summary>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回版本冲突</param>
/// <param name="ParentId">上级组织单元</param>
/// <param name="Name">名称，同一上级下不区分大小写唯一</param>
/// <param name="Code">编码，业务上用来对应其他系统的部门编号，可以不填</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record CreateOrgUnitCommand(long ExpectedVersion, OrgUnitId ParentId, string Name, string? Code) : ICommand<CreatedResponse<OrgUnitId>>;
