namespace Kmic.Operations.Application.Audit;

/// <summary>
/// 审计里记的一个操作对象
/// </summary>
/// <param name="ResourceType">资源类型，如 User</param>
/// <param name="ResourceId">资源 ID</param>
public sealed record AuditTargetItem(string ResourceType, Guid ResourceId);
