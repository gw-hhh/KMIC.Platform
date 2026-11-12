using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 按数据分类处理一个值
/// </summary>
/// <remarks>
/// 脱敏器（<see cref="SecretRedactor" />、<see cref="PersonalRedactor" />、<see cref="InternalRedactor" />）由宿主注册进
/// Compliance 的脱敏器提供者；不经依赖注入、直接处理字符串的地方（日志的解构策略）用这里的方法
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
