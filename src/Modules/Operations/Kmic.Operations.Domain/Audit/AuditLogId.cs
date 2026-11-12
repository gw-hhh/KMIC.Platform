using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Operations.Domain.Audit;

/// <summary>
/// 审计记录的 ID：就是审计消息里的 AuditId，由产生审计的那一端生成
/// </summary>
[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct AuditLogId : IStronglyTypedId
{
    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
