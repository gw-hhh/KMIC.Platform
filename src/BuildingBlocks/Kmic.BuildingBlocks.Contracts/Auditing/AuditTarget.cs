namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 被审计操作作用的一个资源
/// </summary>
/// <param name="ResourceType">资源类型，如 Sample</param>
/// <param name="ResourceId">资源 ID</param>
public sealed record AuditTarget(string ResourceType, Guid ResourceId);
