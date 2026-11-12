using System.ComponentModel.DataAnnotations;
using Kmic.BuildingBlocks.Application.Time;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 访问令牌的验证配置
/// </summary>
public sealed class KmicAuthOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Kmic:Auth";

    /// <summary>
    /// 签发者，固定为本系统
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; init; } = "";

    /// <summary>
    /// 受众，固定为本系统
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Audience { get; init; } = "";

    /// <summary>
    /// 访问令牌的有效期
    /// </summary>
    [TimeSpanRange("00:01:00", "01:00:00")]
    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// 允许的时钟偏差
    /// </summary>
    [TimeSpanRange("00:00:00", "00:05:00")]
    public TimeSpan ClockSkew { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 签名密钥：恰好一个 Active，其余 ValidationOnly；轮换时新旧并存
    /// </summary>
    public IReadOnlyList<SigningKeyOptions> SigningKeys { get; init; } = [];
}
