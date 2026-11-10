namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 一个实体在本次操作中的变更
/// </summary>
/// <param name="Entity">实体类型名</param>
/// <param name="EntityId">实体 ID</param>
/// <param name="Kind">变更方式</param>
/// <param name="Fields">变化的字段；新增时为全部字段，删除时为空</param>
public sealed record EntityChange(string Entity, Guid EntityId, ChangeKind Kind, IReadOnlyList<FieldChange> Fields);
