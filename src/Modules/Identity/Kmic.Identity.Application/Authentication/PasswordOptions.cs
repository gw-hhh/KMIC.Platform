using System.ComponentModel.DataAnnotations;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 密码规则
/// </summary>
/// <remarks>
/// 部署配置，节名 Identity:Password。按 NIST SP 800-63B 的思路：只看长度和常见弱密码，不强制字符组合，不定期强制修改
/// </remarks>
public sealed class PasswordOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Identity:Password";

    /// <summary>
    /// 最短长度
    /// </summary>
    [Range(8, 64)]
    public int MinLength { get; init; } = 10;

    /// <summary>
    /// 最长长度
    /// </summary>
    [Range(64, 1024)]
    public int MaxLength { get; init; } = 128;
}
