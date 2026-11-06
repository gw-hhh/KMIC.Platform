using Vogen;

namespace Spike02.Domain;

/// <summary>
/// 用户 ID（原型：Vogen 默认生成 IComparable，MA0097 按文件名范围关闭）
/// </summary>
[ValueObject]
public readonly partial struct UserId : IStronglyTypedId
{
    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
