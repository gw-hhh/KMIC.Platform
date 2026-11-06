using Vogen;

namespace Spike02.Domain;

/// <summary>
/// 组织单元 ID（原型：Vogen 默认生成 IComparable，MA0097 按文件名范围关闭）
/// </summary>
[ValueObject]
public readonly partial struct OrgUnitId : IStronglyTypedId
{
    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
