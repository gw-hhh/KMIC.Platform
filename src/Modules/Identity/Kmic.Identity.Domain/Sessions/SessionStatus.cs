using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Sessions;

public sealed class SessionStatus : Enumeration<SessionStatus>
{
    public static readonly SessionStatus Active = new("Active", "有效");

    public static readonly SessionStatus Revoked = new("Revoked", "已作废");

    private SessionStatus(string code, string name)
        : base(code, name)
    {
    }
}
