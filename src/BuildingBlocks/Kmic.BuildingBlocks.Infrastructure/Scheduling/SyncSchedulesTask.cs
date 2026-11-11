using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Hosting;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 启动时把定时任务同步到 Quartz
/// </summary>
/// <remarks>
/// 排在权限点同步之后、模块的种子数据之前；调度器在全部启动任务完成后才开始触发
/// </remarks>
internal sealed class SyncSchedulesTask(ScheduleSynchronizer synchronizer) : IStartupTask
{
    public string Name => "Framework.SyncSchedules";

    public int Order => -100;

    public async ValueTask<Result> ExecuteAsync(CancellationToken ct)
    {
        await synchronizer.SyncAsync(BuildInfo.Version, ct);
        return Result.Success();
    }
}
