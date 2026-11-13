using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 放弃之前找出接收方注册的检查并执行
/// </summary>
public interface IAbandonGuard
{
    /// <summary>
    /// 这条收件箱里的死信现在能不能放弃
    /// </summary>
    /// <param name="message">死信的全部内容，要有载荷</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>没有注册检查，或检查通过时成功；检查不通过时是它给的错误；载荷还原不了、消息不在目录里时也失败，不当作可以放弃</returns>
    Task<Result> CheckAsync(DeadMessageDetail message, CancellationToken ct);
}
