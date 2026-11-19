namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 停用、启用组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">组织树的预期版本号</param>
public sealed record OrgUnitVersionRequest(long ExpectedVersion);
