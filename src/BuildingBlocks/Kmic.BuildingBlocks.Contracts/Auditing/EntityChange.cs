namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 实体变更
/// </summary>
/// <param name="Entity">实体类型</param>
/// <param name="EntityId">实体 ID</param>
/// <param name="Kind">变更类型</param>
/// <param name="Fields">变更的字段</param>
public sealed record EntityChange(string Entity, Guid EntityId, ChangeKind Kind, IReadOnlyList<FieldChange> Fields);
