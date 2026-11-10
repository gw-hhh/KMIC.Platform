using Vogen;

namespace Kmic.BuildingBlocks.Domain.Identifiers;

/// <summary>
/// 用户 ID
/// </summary>
/// <remarks>
/// 横切所有模块的身份标识，属于框架概念；对应 Identity 模块中的同名实体
/// </remarks>
[ValueObject<Guid>(
    conversions: Conversions.SystemTextJson | Conversions.TypeConverter,
    parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces,
    toPrimitiveCasting: CastOperator.None,
    fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct UserId : IStronglyTypedId
{
    /// <summary>
    /// 生成新的 ID（UUID v7）
    /// </summary>
    public static UserId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
