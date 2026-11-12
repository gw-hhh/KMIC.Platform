using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 标注属性、字段或参数的数据分类：手机号、邮箱、证件号、地址，日志里部分遮盖
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PersonalDataAttribute() : DataClassificationAttribute(KmicData.Personal);
