namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 停用、启用组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record OrgUnitVersionRequest(long ExpectedVersion);
