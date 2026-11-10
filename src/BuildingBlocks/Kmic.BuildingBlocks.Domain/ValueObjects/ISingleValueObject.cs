namespace Kmic.BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// 只包装一个原始值的值对象（如邮箱、手机号），EF 与 JSON 按约定映射为单列和原始值
/// </summary>
public interface ISingleValueObject<out TValue>
{
    /// <summary>
    /// 包装的原始值，已在工厂方法中规范化
    /// </summary>
    TValue Value { get; }
}
