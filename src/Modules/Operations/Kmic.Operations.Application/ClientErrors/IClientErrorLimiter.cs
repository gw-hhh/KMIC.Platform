using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>按用户与时间窗口限制前端错误上报频率。</summary>
public interface IClientErrorLimiter
{
    /// <summary>判断是否允许本次上报；允许返回 null，超限返回建议等待时长。</summary>
    TimeSpan? TryAcquire(UserId user);
}
