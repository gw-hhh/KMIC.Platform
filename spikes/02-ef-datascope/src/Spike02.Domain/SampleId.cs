using Vogen;

namespace Spike02.Domain;

/// <summary>
/// 示例实体 ID（原型：保留 Vogen 生成的 IComparable，并手写比较运算符，验证键集分页的翻译）
/// </summary>
[ValueObject]
public readonly partial struct SampleId : IStronglyTypedId
{
    public static bool operator <(SampleId left, SampleId right) => left.CompareTo(right) < 0;

    public static bool operator <=(SampleId left, SampleId right) => left.CompareTo(right) <= 0;

    public static bool operator >(SampleId left, SampleId right) => left.CompareTo(right) > 0;

    public static bool operator >=(SampleId left, SampleId right) => left.CompareTo(right) >= 0;

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
