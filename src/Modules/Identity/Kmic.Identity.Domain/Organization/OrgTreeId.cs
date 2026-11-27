using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Identity.Domain.Organization;

/// <summary>组织树 ID；系统仅有一棵组织树，ID 固定。</summary>
[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct OrgTreeId : IStronglyTypedId
{
    public static OrgTreeId Default { get; } = From(new Guid("00000000-0000-0000-0000-000000000003"));

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
