using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>刷新令牌，仅存储哈希并由 <see cref="Session" /> 创建和修改。</summary>
public sealed class RefreshToken : Entity<RefreshTokenId>
{
    private RefreshToken(RefreshTokenId id, string tokenHash, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
        : base(id)
    {
        TokenHash = tokenHash;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// 令牌的 SHA-256 哈希，小写十六进制
    /// </summary>
    public string TokenHash { get; private set; }

    /// <summary>
    /// 签发时间
    /// </summary>
    public DateTimeOffset IssuedAt { get; private set; }

    /// <summary>
    /// 过期时间，不会超过会话的绝对上限
    /// </summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>
    /// 被使用（换成新令牌）的时间；为空表示它是会话当前的令牌
    /// </summary>
    public DateTimeOffset? UsedAt { get; private set; }

    /// <summary>
    /// 接替它的新令牌
    /// </summary>
    public RefreshTokenId? ReplacedById { get; private set; }

    /// <summary>
    /// 是否会话当前的令牌
    /// </summary>
    public bool IsCurrent => UsedAt is null;

    internal static RefreshToken Create(string tokenHash, DateTimeOffset issuedAt, DateTimeOffset expiresAt) =>
        new(RefreshTokenId.New(), tokenHash, issuedAt, expiresAt);

    internal void MarkUsed(DateTimeOffset now, RefreshTokenId replacedBy)
    {
        UsedAt = now;
        ReplacedById = replacedBy;
    }
}
