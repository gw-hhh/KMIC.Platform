using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>
/// 定时任务设置的仓储
/// </summary>
public interface IScheduledJobSettingRepository : IRepository<ScheduledJobSetting, ScheduledJobSettingId>
{
    /// <summary>
    /// 按任务名取要修改的设置：没有这一行（任务一直用默认值）时得到空
    /// </summary>
    /// <param name="jobName">任务名</param>
    /// <param name="expectedVersion">调用方看到的版本号；空表示调用方看到的是没有设置，这时已经有这一行就是版本冲突</param>
    /// <param name="ct">取消令牌</param>
    Task<Result<ScheduledJobSetting?>> GetByJobNameForUpdateAsync(string jobName, long? expectedVersion, CancellationToken ct);
}
