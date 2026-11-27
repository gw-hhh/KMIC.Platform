using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Impersonation.End;

/// <summary>结束当前身份模拟会话（只读模拟会话亦可调用）</summary>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[AllowWhenImpersonationReadOnly]
[NotAudited("通过 ImpersonationEnded 安全事件记录模拟结束")]
[ExemptFromMessagingSuspension]
public sealed record EndImpersonationCommand : ICommand;
