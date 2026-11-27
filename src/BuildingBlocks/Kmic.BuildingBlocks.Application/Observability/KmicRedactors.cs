using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>供非依赖注入场景按数据分类脱敏的工具。</summary>
public static class KmicRedactors
{
    /// <summary>Secret 输出 ***，Personal 部分遮盖，其余原样输出。</summary>
    public static string Redact(string value, DataClassification classification)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (classification == KmicData.Secret)
        {
            return SecretRedactor.Mask;
        }

        return classification == KmicData.Personal ? PartialMask.Of(value) : value;
    }
}
