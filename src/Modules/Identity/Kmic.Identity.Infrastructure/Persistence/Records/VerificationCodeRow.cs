namespace Kmic.Identity.Infrastructure.Persistence.Records;

/// <summary>
/// 短信验证码记录
/// </summary>
internal sealed class VerificationCodeRow
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 手机号哈希，SHA-256 小写十六进制
    /// </summary>
    public required string PhoneHash { get; init; }

    /// <summary>
    /// 关联用户 ID
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
    /// 剩余校验次数
    /// </summary>
    public int AttemptsLeft { get; init; }

    /// <summary>
    /// 状态：Pending、Used、Expired、Exhausted
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// 请求来源 IP
    /// </summary>
    public string? RequestIp { get; init; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}
