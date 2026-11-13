using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 前端错误上报的限流：每个用户每分钟最多几次请求
/// </summary>
public interface IClientErrorLimiter
{
    /// <summary>
    /// 这个用户这次请求放不放行
    /// </summary>
    /// <param name="user">上报的用户</param>
    /// <returns>放行时为空；超限时是多久以后可以再报</returns>
    TimeSpan? TryAcquire(UserId user);
}
