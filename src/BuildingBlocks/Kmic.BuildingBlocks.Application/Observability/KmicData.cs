using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 数据分类：同一个字段在日志与审计里按同一套规则处理
/// </summary>
/// <remarks>
/// 脱敏的依据是“数据是什么”的声明，不是对文本内容的猜测。没有声明的数据按 Internal 处理，原样输出，
/// 所以手机号、令牌这类数据必须声明
/// </remarks>
public static class KmicData
{
    private const string Taxonomy = "Kmic";

    /// <summary>
    /// 密码、令牌、密钥、验证码：日志里输出 ***，审计里只记“已修改”
    /// </summary>
    public static DataClassification Secret { get; } = new(Taxonomy, nameof(Secret));

    /// <summary>
    /// 手机号、邮箱、证件号、地址：部分遮盖，如 138****5678
    /// </summary>
    public static DataClassification Personal { get; } = new(Taxonomy, nameof(Personal));

    /// <summary>
    /// 内部 ID、业务编号：原样输出
    /// </summary>
    public static DataClassification Internal { get; } = new(Taxonomy, nameof(Internal));
}
