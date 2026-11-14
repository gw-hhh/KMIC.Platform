using System.ComponentModel.DataAnnotations;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 密码规则
/// </summary>
/// <remarks>
/// 配置节为 Identity:Password，设置密码长度范围。密码策略另行检查常见弱密码及与账号、手机号重复的情况，不强制字符组合或定期改密
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
