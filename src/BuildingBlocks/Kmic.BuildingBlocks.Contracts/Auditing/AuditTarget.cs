namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 操作对象
/// </summary>
/// <param name="ResourceType">资源类型，如 Sample</param>
/// <param name="ResourceId">资源 ID</param>
public sealed record AuditTarget(string ResourceType, Guid ResourceId);
