namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 修改组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">组织树的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="Name">名称，最多 100 个字符；同级不得重名（含停用单元，不区分大小写）</param>
/// <param name="Code">编码，最多 50 个字符；不填表示清空</param>
public sealed record UpdateOrgUnitRequest(long ExpectedVersion, string Name, string? Code);
