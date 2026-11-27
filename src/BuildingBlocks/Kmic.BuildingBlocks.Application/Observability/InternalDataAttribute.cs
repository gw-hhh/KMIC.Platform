using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>将数据分类标为内部数据（如内部 ID、业务编号），日志中原样输出。</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class InternalDataAttribute() : DataClassificationAttribute(KmicData.Internal);
