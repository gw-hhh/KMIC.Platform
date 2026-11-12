using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 刷新令牌 ID
/// </summary>
[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct RefreshTokenId : IStronglyTypedId
{
    /// <summary>
    /// 生成新 ID
    /// </summary>
    public static RefreshTokenId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
