using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Execution;

public interface ICurrentUser
{
    /// <summary>用户 ID；系统身份或匿名请求时为 null。</summary>
    UserId? UserId { get; }

    RoleId? RoleId { get; }

    bool IsSystem { get; }

    bool IsImpersonating { get; }

    UserId? ActualUserId { get; }

    SessionId? SessionId { get; }

    /// <summary>原始操作人（以系统身份处理事件时最初触发该事件的用户），仅用于审计追踪。</summary>
    UserId? OnBehalfOf { get; }
}
