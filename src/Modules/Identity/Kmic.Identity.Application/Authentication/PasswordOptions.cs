using System.ComponentModel.DataAnnotations;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 密码长度规则
/// </summary>
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
