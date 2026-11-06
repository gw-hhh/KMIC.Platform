namespace Spike03.Domain;

/// <summary>
/// 只包一个原始值的值对象，EF 与 JSON 按约定映射为单个原始值（05-01 D4）
/// </summary>
public interface ISingleValueObject<out TValue>
{
    TValue Value { get; }
}
