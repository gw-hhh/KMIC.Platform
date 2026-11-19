using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 添加组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">组织树的预期版本号</param>
/// <param name="ParentId">上级组织单元</param>
/// <param name="Name">名称</param>
/// <param name="Code">编码</param>
public sealed record CreateOrgUnitRequest(long ExpectedVersion, OrgUnitId ParentId, string Name, string? Code);
