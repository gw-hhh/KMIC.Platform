using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 按提交人判断作业归属，跨用户查询使用管理接口
/// </summary>
internal static class MyJobs
{
    /// <summary>
    /// 取当前用户提交的作业；模块或作业不存在、不是自己提交的都返回空
    /// </summary>
    /// <remarks>
    /// 模拟登录时按被模拟的人算，和“我的作业”列表一致
    /// </remarks>
    public static async Task<JobView?> FindAsync(IExecutionContext execution, IModuleJobStores stores, string module, Guid jobId, CancellationToken ct)
    {
        if (execution.User.UserId is not { } me || stores.Find(module) is not { } store)
        {
            return null;
        }

        var job = await store.FindAsync(jobId, ct);
        return job is { SubmittedBy: { } by } && by == me.Value ? job : null;
    }
}
