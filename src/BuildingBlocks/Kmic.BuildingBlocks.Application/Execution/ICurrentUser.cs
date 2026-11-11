using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 当前身份
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// 用户 ID，系统身份和匿名请求时为空
    /// </summary>
    UserId? UserId { get; }

    /// <summary>
    /// 角色
    /// </summary>
    RoleId? RoleId { get; }

    /// <summary>
    /// 是否系统身份
    /// </summary>
    bool IsSystem { get; }

    /// <summary>
    /// 是否在模拟登录
    /// </summary>
    bool IsImpersonating { get; }

    /// <summary>
    /// 实际操作人
    /// </summary>
    UserId? ActualUserId { get; }

    /// <summary>
    /// 会话
    /// </summary>
    SessionId? SessionId { get; }

    /// <summary>
    /// 原始操作人
    /// </summary>
    /// <remarks>
    /// 系统身份处理事件时，引发事件的用户。只用于审计
    /// </remarks>
    UserId? OnBehalfOf { get; }
}
