using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Organization.Create;

/// <summary>在已启用的组织单元下新增子级</summary>
/// <param name="ExpectedVersion">组织树的预期版本号</param>
/// <param name="ParentId">上级组织单元</param>
/// <param name="Name">名称</param>
/// <param name="Code">编码</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record CreateOrgUnitCommand(long ExpectedVersion, OrgUnitId ParentId, string Name, string? Code) : ICommand<CreatedResponse<OrgUnitId>>;
