using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Configuration.Domain.Dictionaries;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct DictionaryItemId : IStronglyTypedId
{
    public static DictionaryItemId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
