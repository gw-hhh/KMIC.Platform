using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Notifications.Domain.Captured;

/// <summary>
/// 非生产环境捕获的一条内容的 ID
/// </summary>
[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct CapturedMessageId : IStronglyTypedId
{
    /// <summary>
    /// 生成新 ID
    /// </summary>
    public static CapturedMessageId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
