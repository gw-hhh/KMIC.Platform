namespace Kmic.Operations.Application.Audit;

/// <summary>审计记录中的操作对象。</summary>
/// <param name="ResourceType">资源类型</param>
/// <param name="ResourceId">资源 ID</param>
public sealed record AuditTargetItem(string ResourceType, Guid ResourceId);
