using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

internal static class MyJobs
{
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
