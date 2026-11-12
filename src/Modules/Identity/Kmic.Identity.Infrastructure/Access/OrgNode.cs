namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 组织树快照里的一个单元
/// </summary>
/// <param name="Id">单元 ID</param>
/// <param name="ParentId">上级 ID</param>
internal sealed record OrgNode(Guid Id, Guid? ParentId);
