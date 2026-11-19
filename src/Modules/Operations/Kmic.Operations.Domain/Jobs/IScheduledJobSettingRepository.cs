using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>
/// 定时任务设置的仓储
/// </summary>
public interface IScheduledJobSettingRepository : IRepository<ScheduledJobSetting, ScheduledJobSettingId>
{
    /// <summary>
    /// 按任务名读取待修改的设置
    /// </summary>
    /// <remarks>
    /// 不指定版本时要求还没有记录
    /// </remarks>
    Task<Result<ScheduledJobSetting?>> GetByJobNameForUpdateAsync(string jobName, long? expectedVersion, CancellationToken ct);
}
