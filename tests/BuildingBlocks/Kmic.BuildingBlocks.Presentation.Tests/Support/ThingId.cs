using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
internal readonly partial struct ThingId : IStronglyTypedId
{
    public static ThingId New() => From(Guid.CreateVersion7());
}
