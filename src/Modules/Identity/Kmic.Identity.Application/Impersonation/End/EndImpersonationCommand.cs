using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Impersonation.End;

/// <summary>
/// 结束当前的模拟会话
/// </summary>
/// <remarks>
/// 只在模拟会话里有效。不改业务数据，所以只读的模拟会话里也能执行。前端同时丢弃模拟令牌
/// </remarks>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[AllowWhenImpersonationReadOnly]
[NotAudited("通过 ImpersonationEnded 安全事件记录模拟结束")]
public sealed record EndImpersonationCommand : ICommand;
