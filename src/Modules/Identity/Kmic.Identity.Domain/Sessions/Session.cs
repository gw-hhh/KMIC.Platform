using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 会话：一次登录的服务端记录
/// </summary>
/// <remarks>
/// 访问令牌里只有用户、会话和角色三个标识，每个请求按会话状态判断是否还有效，
/// 所以作废会话立即生效，不必等访问令牌过期。刷新令牌每次换新，旧令牌在宽限期之外再次出现就作废整个会话
/// </remarks>
public sealed class Session : AggregateRoot<SessionId>
{
    private readonly List<RefreshToken> _refreshTokens = [];

    private Session(SessionId id, UserId userId, RoleId currentRoleId, ClientType clientType, string? deviceName, string? loginIp)
        : base(id)
    {
        UserId = userId;
        CurrentRoleId = currentRoleId;
        ClientType = clientType;
        DeviceName = deviceName;
        LoginIp = loginIp;
    }

    /// <summary>
    /// 用户；模拟会话里是被模拟的人
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// 当前角色
    /// </summary>
    public RoleId CurrentRoleId { get; private set; }

    /// <summary>
    /// 客户端类型
    /// </summary>
    public ClientType ClientType { get; private set; }

    /// <summary>
    /// 设备名称，App 上报或由 User-Agent 解析
    /// </summary>
    public string? DeviceName { get; private set; }

    /// <summary>
    /// 登录时的客户端 IP
    /// </summary>
    public string? LoginIp { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public SessionStatus Status { get; private set; } = SessionStatus.Active;

    /// <summary>
    /// 作废原因
    /// </summary>
    public RevokeReason? RevokedReason { get; private set; }

    /// <summary>
    /// 作废时间
    /// </summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// 最近一次刷新的时间，只在刷新时更新，不在每个请求更新
    /// </summary>
    public DateTimeOffset LastRefreshedAt { get; private set; }

    /// <summary>
    /// 滑动过期时间
    /// </summary>
    public DateTimeOffset RefreshExpiresAt { get; private set; }

    /// <summary>
    /// 绝对上限：无论怎样刷新，会话最晚在这个时间失效
    /// </summary>
    public DateTimeOffset AbsoluteExpiresAt { get; private set; }

    /// <summary>
    /// 模拟身份时的实际操作人；普通会话为空
    /// </summary>
    public UserId? ImpersonatorUserId { get; private set; }

    /// <summary>
    /// 模拟会话是否允许修改数据，创建时确定，会话期间不变
    /// </summary>
    public bool ImpersonationAllowsWrite { get; private set; }

    /// <summary>
    /// 开始模拟的原因
    /// </summary>
    public string? ImpersonationReason { get; private set; }

    /// <summary>
    /// 刷新令牌；模拟会话没有
    /// </summary>
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens;

    /// <summary>
    /// 是否模拟会话
    /// </summary>
    public bool IsImpersonation => ImpersonatorUserId is not null;

    /// <summary>
    /// 开始一个普通会话，同时签发第一个刷新令牌
    /// </summary>
    public static Session Start(
        UserId userId,
        RoleId roleId,
        ClientType clientType,
        string? deviceName,
        string? loginIp,
        string refreshTokenHash,
        DateTimeOffset now,
        TimeSpan refreshLifetime,
        TimeSpan absoluteLifetime)
    {
        ArgumentNullException.ThrowIfNull(clientType);
        ArgumentNullException.ThrowIfNull(refreshTokenHash);

        var absolute = now + absoluteLifetime;
        var refreshExpires = Earlier(now + refreshLifetime, absolute);
        var session = new Session(SessionId.New(), userId, roleId, clientType, deviceName, loginIp)
        {
            LastRefreshedAt = now,
            RefreshExpiresAt = refreshExpires,
            AbsoluteExpiresAt = absolute,
        };
        session._refreshTokens.Add(RefreshToken.Create(refreshTokenHash, now, refreshExpires));
        return session;
    }

    /// <summary>
    /// 开始一个模拟会话：不签发刷新令牌，到期就结束，不超过 <paramref name="lifetime" />
    /// </summary>
    public static Session StartImpersonation(
        UserId targetUserId,
        RoleId roleId,
        UserId impersonatorUserId,
        bool allowsWrite,
        string reason,
        ClientType clientType,
        string? deviceName,
        string? loginIp,
        DateTimeOffset now,
        TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentNullException.ThrowIfNull(clientType);

        var expires = now + lifetime;
        return new Session(SessionId.New(), targetUserId, roleId, clientType, deviceName, loginIp)
        {
            LastRefreshedAt = now,
            RefreshExpiresAt = expires,
            AbsoluteExpiresAt = expires,
            ImpersonatorUserId = impersonatorUserId,
            ImpersonationAllowsWrite = allowsWrite,
            ImpersonationReason = reason.Trim(),
        };
    }

    /// <summary>
    /// 此刻是否还能用：没有作废，也没有超过绝对上限
    /// </summary>
    public bool IsUsable(DateTimeOffset now) => Status == SessionStatus.Active && now < AbsoluteExpiresAt;

    /// <summary>
    /// 用刷新令牌换新令牌
    /// </summary>
    /// <remarks>
    /// 会话当前的令牌正常轮换；刚被轮换掉的旧令牌在宽限期内再次出现，视为多个标签页并发刷新，
    /// 从当前的令牌继续轮换；宽限期之外的旧令牌再次出现是重用，整个会话作废
    /// </remarks>
    /// <param name="presentedHash">客户端出示的令牌的哈希</param>
    /// <param name="newHash">新令牌的哈希</param>
    /// <param name="now">当前时间</param>
    /// <param name="refreshLifetime">滑动有效期</param>
    /// <param name="grace">并发刷新的宽限期</param>
    public Rotation Rotate(string presentedHash, string newHash, DateTimeOffset now, TimeSpan refreshLifetime, TimeSpan grace)
    {
        ArgumentNullException.ThrowIfNull(presentedHash);
        ArgumentNullException.ThrowIfNull(newHash);

        var presented = _refreshTokens.Find(t => string.Equals(t.TokenHash, presentedHash, StringComparison.Ordinal));
        if (presented is null || IsImpersonation || !IsUsable(now) || now >= presented.ExpiresAt)
        {
            return Rotation.Invalid();
        }

        var current = presented;
        if (!presented.IsCurrent)
        {
            if (now - presented.UsedAt!.Value > grace)
            {
                Revoke(RevokeReason.RefreshTokenReused, now);
                return Rotation.Reused();
            }

            current = _refreshTokens.Find(t => t.IsCurrent);
            if (current is null)
            {
                return Rotation.Invalid();
            }
        }

        var expires = Earlier(now + refreshLifetime, AbsoluteExpiresAt);
        var replacement = RefreshToken.Create(newHash, now, expires);
        current.MarkUsed(now, replacement.Id);
        _refreshTokens.Add(replacement);
        LastRefreshedAt = now;
        RefreshExpiresAt = expires;
        return Rotation.Rotated(replacement);
    }

    /// <summary>
    /// 切换当前角色；角色是否可切换由调用方先确认
    /// </summary>
    public void SwitchRole(RoleId roleId) => CurrentRoleId = roleId;

    /// <summary>
    /// 作废；已经作废的保持最初的原因
    /// </summary>
    public void Revoke(RevokeReason reason, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(reason);

        if (Status == SessionStatus.Revoked)
        {
            return;
        }

        Status = SessionStatus.Revoked;
        RevokedReason = reason;
        RevokedAt = now;
    }

    private static DateTimeOffset Earlier(DateTimeOffset a, DateTimeOffset b) => a <= b ? a : b;
}
