using Vogen;

namespace Kmic.BuildingBlocks.Domain.Identifiers;

/// <summary>用户 ID。</summary>
[ValueObject<Guid>(
    conversions: Conversions.SystemTextJson | Conversions.TypeConverter,
    parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces,
    toPrimitiveCasting: CastOperator.None,
    fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct UserId : IStronglyTypedId
{
    /// <summary>系统内置用户 ID，无登录用户上下文时审计字段记为此值。</summary>
    public static UserId System { get; } = From(new Guid("00000000-0000-0000-0000-000000000001"));

    public static UserId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
