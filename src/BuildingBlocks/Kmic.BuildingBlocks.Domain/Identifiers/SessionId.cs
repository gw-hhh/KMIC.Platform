using Vogen;

namespace Kmic.BuildingBlocks.Domain.Identifiers;

/// <summary>
/// 会话 ID
/// </summary>
[ValueObject<Guid>(
    conversions: Conversions.SystemTextJson | Conversions.TypeConverter,
    parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces,
    toPrimitiveCasting: CastOperator.None,
    fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct SessionId : IStronglyTypedId
{
    /// <summary>
    /// 生成新 ID
    /// </summary>
    public static SessionId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
