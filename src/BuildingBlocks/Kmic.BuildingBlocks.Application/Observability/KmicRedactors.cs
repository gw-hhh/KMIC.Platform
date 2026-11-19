using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 按数据分类处理一个值
/// </summary>
/// <remarks>
/// 给不经依赖注入的地方用，如日志的解构策略
/// </remarks>
public static class KmicRedactors
{
    /// <summary>
    /// Secret 输出 ***，Personal 部分遮盖，其余原样
    /// </summary>
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
