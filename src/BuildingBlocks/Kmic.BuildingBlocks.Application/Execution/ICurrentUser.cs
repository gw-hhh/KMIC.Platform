using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 当前执行身份
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// 用户；系统身份与匿名请求为空
    /// </summary>
    UserId? UserId { get; }

    /// <summary>
    /// 当前角色
    /// </summary>
    RoleId? RoleId { get; }

    /// <summary>
    /// 是否系统身份（定时任务、事件处理、启动任务）
    /// </summary>
    bool IsSystem { get; }

    /// <summary>
    /// 是否在模拟其他用户
    /// </summary>
    bool IsImpersonating { get; }

    /// <summary>
    /// 模拟身份时的实际操作人，否则为空
    /// </summary>
    UserId? ActualUserId { get; }

    /// <summary>
    /// 会话
    /// </summary>
    SessionId? SessionId { get; }

    /// <summary>
    /// 系统身份处理事件时，引发它的原始操作人；只用于审计
    /// </summary>
    UserId? OnBehalfOf { get; }
}
