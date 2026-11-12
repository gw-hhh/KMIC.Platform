using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 结束自己的一个登录设备
/// </summary>
/// <remarks>
/// 只能结束自己的会话；别人的会话和不存在的会话一样返回找不到。结束当前会话等于退出登录。天然幂等：会话已经结束时再结束不改变任何东西
/// </remarks>
/// <param name="SessionId">要结束的会话</param>
[AllowAuthenticated]
[NotAudited("结果记入安全事件 SessionRevoked")]
[NotIdempotent("结束会话天然幂等：已经结束时再结束不改变任何东西")]
public sealed record RevokeMySessionCommand(SessionId SessionId) : ICommand;
