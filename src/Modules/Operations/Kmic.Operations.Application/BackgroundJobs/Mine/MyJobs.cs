using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// “我的作业”的归属判断：只认提交人，管理员看别人的作业走管理视图
/// </summary>
internal static class MyJobs
{
    /// <summary>
    /// 取当前用户提交的作业；模块不存在、作业不存在、不是自己提交的都返回空，调用方无法区分
    /// </summary>
    /// <remarks>
    /// 模拟登录时当前用户是被模拟的人，和“我的作业”列表一致
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
