using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Execution;

public sealed record CurrentUser : ICurrentUser
{
    private CurrentUser()
    {
    }

    public static CurrentUser Anonymous { get; } = new();

    public UserId? UserId { get; private init; }

    public RoleId? RoleId { get; private init; }

    public bool IsSystem { get; private init; }

    public bool IsImpersonating => ActualUserId is not null;

    public UserId? ActualUserId { get; private init; }

    public SessionId? SessionId { get; private init; }

    public UserId? OnBehalfOf { get; private init; }

    public static CurrentUser System(UserId? onBehalfOf = null) => new() { IsSystem = true, OnBehalfOf = onBehalfOf };

    public static CurrentUser From(IAccessContext access)
    {
        ArgumentNullException.ThrowIfNull(access);
        return new()
        {
            UserId = access.UserId,
            RoleId = access.RoleId,
            SessionId = access.SessionId,
            ActualUserId = access.ImpersonatorUserId,
        };
    }
}
