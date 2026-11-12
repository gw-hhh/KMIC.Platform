using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 会话的状态
/// </summary>
public sealed class SessionStatus : Enumeration<SessionStatus>
{
    /// <summary>
    /// 有效；超过绝对上限的会话也算失效，那是加载时按时间判断的，不改状态
    /// </summary>
    public static readonly SessionStatus Active = new("Active", "有效");

    /// <summary>
    /// 已作废
    /// </summary>
    public static readonly SessionStatus Revoked = new("Revoked", "已作废");

    private SessionStatus(string code, string name)
        : base(code, name)
    {
    }
}
