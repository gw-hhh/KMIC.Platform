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
    /// 未指定 expectedVersion 时要求尚无设置记录，并返回 null；指定版本时要求记录存在且版本一致，否则返回版本冲突
    /// </remarks>
    Task<Result<ScheduledJobSetting?>> GetByJobNameForUpdateAsync(string jobName, long? expectedVersion, CancellationToken ct);
}
