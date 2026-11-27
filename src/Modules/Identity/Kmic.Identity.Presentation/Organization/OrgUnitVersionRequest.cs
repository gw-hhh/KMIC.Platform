namespace Kmic.Identity.Presentation.Organization;

/// <param name="ExpectedVersion">组织树的预期版本号</param>
public sealed record OrgUnitVersionRequest(long ExpectedVersion);
