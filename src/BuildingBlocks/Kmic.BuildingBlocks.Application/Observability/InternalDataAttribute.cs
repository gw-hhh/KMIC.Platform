using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 标注属性、字段或参数的数据分类：内部 ID、业务编号，日志里原样输出
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class InternalDataAttribute() : DataClassificationAttribute(KmicData.Internal);
