using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.Identity.Domain.Organization;

/// <summary>
/// 组织树 ID
/// </summary>
/// <remarks>
/// 系统里只有一棵组织树，ID 固定
/// </remarks>
[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces, toPrimitiveCasting: CastOperator.None, fromPrimitiveCasting: CastOperator.None)]
public readonly partial struct OrgTreeId : IStronglyTypedId
{
    /// <summary>
    /// 唯一的组织树
    /// </summary>
    public static OrgTreeId Default { get; } = From(new Guid("00000000-0000-0000-0000-000000000003"));

    private static Validation Validate(Guid value) => value == Guid.Empty ? Validation.Invalid("ID 不能为空") : Validation.Ok;
}
