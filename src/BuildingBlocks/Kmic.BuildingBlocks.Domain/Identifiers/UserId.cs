using Vogen;

namespace Kmic.BuildingBlocks.Domain.Identifiers;

/// <summary>
/// 用户 ID
/// </summary>
[ValueObject<Guid>(
    conversions: Conversions.SystemTextJson | Conversions.TypeConverter,
    parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces,
    toPrimitiveCasting: CastOperator.None,
    fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct UserId : IStronglyTypedId
{
    /// <summary>
    /// 系统用户
    /// </summary>
    /// <remarks>
    /// 定时任务、事件处理这类没有登录用户的操作，审计列记为系统用户
    /// </remarks>
    public static UserId System { get; } = From(new Guid("00000000-0000-0000-0000-000000000001"));

    /// <summary>
    /// 生成新 ID
    /// </summary>
    public static UserId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
