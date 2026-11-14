using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Operations.Domain.Audit;

/// <summary>
/// 审计记录 ID，沿用审计消息中的 AuditId
/// </summary>
[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct AuditLogId : IStronglyTypedId
{
    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
