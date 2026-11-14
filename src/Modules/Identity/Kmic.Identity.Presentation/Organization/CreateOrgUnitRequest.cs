using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 添加组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">组织树的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="ParentId">上级组织单元，必须启用</param>
/// <param name="Name">名称，最多 100 个字符，同一上级下不区分大小写唯一</param>
/// <param name="Code">编码，最多 50 个字符，可以不填</param>
public sealed record CreateOrgUnitRequest(long ExpectedVersion, OrgUnitId ParentId, string Name, string? Code);
