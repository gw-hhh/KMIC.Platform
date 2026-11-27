using System.ComponentModel.DataAnnotations;
using Kmic.BuildingBlocks.Application.Time;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>访问令牌签发与验证配置</summary>
public sealed class KmicAuthOptions
{
    public const string Section = "Kmic:Auth";

    /// <summary>令牌签发者</summary>
    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; init; } = "";

    /// <summary>令牌受众</summary>
    [Required(AllowEmptyStrings = false)]
    public string Audience { get; init; } = "";

    /// <summary>访问令牌有效期</summary>
    [TimeSpanRange("00:01:00", "01:00:00")]
    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>允许的时钟偏差</summary>
    [TimeSpanRange("00:00:00", "00:05:00")]
    public TimeSpan ClockSkew { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>签名密钥列表（须恰好有一个 Active，其余为 ValidationOnly）</summary>
    public IReadOnlyList<SigningKeyOptions> SigningKeys { get; init; } = [];
}
