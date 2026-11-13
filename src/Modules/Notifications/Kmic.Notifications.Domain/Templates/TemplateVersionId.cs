using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>
/// 模板版本 ID
/// </summary>
[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct TemplateVersionId : IStronglyTypedId
{
    /// <summary>
    /// 生成新 ID
    /// </summary>
    public static TemplateVersionId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
