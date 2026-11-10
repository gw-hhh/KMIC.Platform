using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 执行身份的标准实现；由各入口在建立执行上下文时创建
/// </summary>
public sealed record CurrentUser : ICurrentUser
{
    private CurrentUser()
    {
    }

    /// <summary>
    /// 匿名请求
    /// </summary>
    public static CurrentUser Anonymous { get; } = new();

    /// <inheritdoc />
    public UserId? UserId { get; private init; }

    /// <inheritdoc />
    public RoleId? RoleId { get; private init; }

    /// <inheritdoc />
    public bool IsSystem { get; private init; }

    /// <inheritdoc />
    public bool IsImpersonating => ActualUserId is not null;

    /// <inheritdoc />
    public UserId? ActualUserId { get; private init; }

    /// <inheritdoc />
    public SessionId? SessionId { get; private init; }

    /// <inheritdoc />
    public UserId? OnBehalfOf { get; private init; }

    /// <summary>
    /// 系统身份；处理事件时带上引发它的原始操作人
    /// </summary>
    public static CurrentUser System(UserId? onBehalfOf = null) => new() { IsSystem = true, OnBehalfOf = onBehalfOf };

    /// <summary>
    /// 来自访问上下文的用户身份
    /// </summary>
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
