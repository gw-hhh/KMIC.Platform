using System.ComponentModel.DataAnnotations;

namespace Kmic.Identity.Application.Authentication;

/// <summary>密码长度策略配置</summary>
public sealed class PasswordOptions
{
    public const string Section = "Identity:Password";

    /// <summary>最短长度</summary>
    [Range(12, 64)]
    public int MinLength { get; init; } = 12;

    /// <summary>最长长度</summary>
    [Range(64, 1024)]
    public int MaxLength { get; init; } = 128;
}
