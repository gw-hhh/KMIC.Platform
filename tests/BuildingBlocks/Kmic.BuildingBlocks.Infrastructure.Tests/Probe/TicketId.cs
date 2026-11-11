using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
internal readonly partial struct TicketId : IStronglyTypedId
{
    public static TicketId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
