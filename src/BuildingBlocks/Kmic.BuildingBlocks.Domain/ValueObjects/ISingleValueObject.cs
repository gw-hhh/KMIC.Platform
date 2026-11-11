namespace Kmic.BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// 单值值对象，如邮箱、手机号
/// </summary>
public interface ISingleValueObject<out TValue>
{
    /// <summary>
    /// 原始值
    /// </summary>
    TValue Value { get; }
}
