using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Notifications.Domain.Notifications;

/// <summary>
/// 一次发送请求的记录 ID
/// </summary>
[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct NotificationId : IStronglyTypedId
{
    /// <summary>
    /// 生成新 ID
    /// </summary>
    public static NotificationId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
