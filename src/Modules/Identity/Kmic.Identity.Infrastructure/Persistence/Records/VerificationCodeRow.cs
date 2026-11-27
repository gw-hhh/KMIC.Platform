namespace Kmic.Identity.Infrastructure.Persistence.Records;

// 短信验证码预留表映射，流程尚未接入
internal sealed class VerificationCodeRow
{
    public Guid Id { get; init; }

    // 手机号 SHA-256（小写十六进制）
    public required string PhoneHash { get; init; }

    public Guid? UserId { get; init; }

    // 用途：Login / ResetPassword
    public required string Purpose { get; init; }

    // 验证码 SHA-256（小写十六进制）
    public required string CodeHash { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public int AttemptsLeft { get; init; }

    // 状态：Pending / Used / Expired / Exhausted
    public required string Status { get; init; }

    public string? RequestIp { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
