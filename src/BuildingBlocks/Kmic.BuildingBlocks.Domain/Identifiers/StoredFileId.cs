using Vogen;

namespace Kmic.BuildingBlocks.Domain.Identifiers;

/// <summary>
/// 文件 ID：Files 模块里一个文件的标识，各模块引用文件时用它（跨模块引用，所以放在框架里）
/// </summary>
[ValueObject<Guid>(
    conversions: Conversions.SystemTextJson | Conversions.TypeConverter,
    parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces,
    toPrimitiveCasting: CastOperator.None,
    fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct StoredFileId : IStronglyTypedId
{
    /// <summary>
    /// 生成新 ID
    /// </summary>
    public static StoredFileId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
