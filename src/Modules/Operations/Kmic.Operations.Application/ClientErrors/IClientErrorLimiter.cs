using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 按用户和分钟限制前端错误上报请求数
/// </summary>
public interface IClientErrorLimiter
{
    /// <summary>
    /// 判断是否允许用户本次上报
    /// </summary>
    /// <param name="user">上报的用户</param>
    /// <returns>允许时返回 null；超限时返回建议等待时长</returns>
    TimeSpan? TryAcquire(UserId user);
}
