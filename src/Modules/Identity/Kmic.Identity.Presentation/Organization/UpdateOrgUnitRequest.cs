namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 修改组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">组织树的预期版本号</param>
/// <param name="Name">名称</param>
/// <param name="Code">编码</param>
public sealed record UpdateOrgUnitRequest(long ExpectedVersion, string Name, string? Code);
