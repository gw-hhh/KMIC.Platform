namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 修改组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Name">名称，最多 100 个字符，同一上级下不区分大小写唯一</param>
/// <param name="Code">编码，最多 50 个字符；不填表示清空</param>
public sealed record UpdateOrgUnitRequest(long ExpectedVersion, string Name, string? Code);
