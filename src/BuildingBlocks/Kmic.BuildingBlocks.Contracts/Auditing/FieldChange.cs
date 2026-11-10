namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 一个字段的原值与新值
/// </summary>
/// <param name="Field">字段名</param>
/// <param name="Old">原值的文本形式，新增时为空</param>
/// <param name="New">新值的文本形式，删除时为空</param>
/// <param name="Masked">值是否已被遮盖或隐藏</param>
public sealed record FieldChange(string Field, string? Old, string? New, bool Masked);
