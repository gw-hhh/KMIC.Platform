namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 字段变更
/// </summary>
/// <param name="Field">字段名</param>
/// <param name="Old">原值</param>
/// <param name="New">新值</param>
/// <param name="Masked">是否已脱敏</param>
public sealed record FieldChange(string Field, string? Old, string? New, bool Masked);
