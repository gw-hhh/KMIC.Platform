namespace Kmic.Identity.Infrastructure.Persistence.Records;

/// <summary>
/// 短信验证码表的一行：生命周期很短，只存哈希，用原子 SQL 校验与扣减次数
/// </summary>
internal sealed class VerificationCodeRow
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 手机号的 SHA-256，小写十六进制：手机号不存明文，不存在的号码也写一行，使频率限制对存在与不存在的号码一致
    /// </summary>
    public required string PhoneHash { get; init; }

    /// <summary>
    /// 用户；手机号不属于任何用户时为空，这一行不生成验证码
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// 用途：Login 或 ResetPassword
    /// </summary>
    public required string Purpose { get; init; }

    /// <summary>
    /// 验证码的哈希，小写十六进制
    /// </summary>
    public required string CodeHash { get; init; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// 还能校验几次
    /// </summary>
    public int AttemptsLeft { get; init; }

    /// <summary>
    /// 状态：Pending、Used、Expired、Exhausted
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// 请求来源 IP，按 IP 限流用
    /// </summary>
    public string? RequestIp { get; init; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}
