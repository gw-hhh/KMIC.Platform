using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>数据分类定义（日志与审计共用）；未声明字段默认按 Internal 原样输出，敏感信息须显式标注。</summary>
public static class KmicData
{
    private const string Taxonomy = "Kmic";

    /// <summary>敏感凭据（密码、令牌、密钥、验证码）：日志脱敏为 ***，审计仅记录“已修改”。</summary>
    public static DataClassification Secret { get; } = new(Taxonomy, nameof(Secret));

    /// <summary>个人数据（手机号、邮箱、证件号、地址）：部分遮盖（如 138****5678）。</summary>
    public static DataClassification Personal { get; } = new(Taxonomy, nameof(Personal));

    /// <summary>内部数据（内部 ID、业务编号）：原样输出。</summary>
    public static DataClassification Internal { get; } = new(Taxonomy, nameof(Internal));
}
