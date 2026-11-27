using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Background;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Infrastructure.Deletion;

// 处理文件删除队列：在数据库事务外先删除存储对象，再更新数据库记录
internal sealed class StoredFileDeletionWork(
    IExecutionScopeFactory scopes,
    IFileStorage storage,
    IOptions<FilesDeletionOptions> options,
    ILogger<StoredFileDeletionWork> logger)
    : ILeasedWork<DeletionItem>
{
    public string Name => "Files.Deletion";

    public int Parallelism => options.Value.Parallelism;

    public TimeSpan PollInterval => options.Value.Poll;

    public async Task<IReadOnlyList<DeletionItem>> ClaimAsync(int max, Guid token, CancellationToken ct)
    {
        await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, CurrentUser.System());
        return await ActivatorUtilities.CreateInstance<FilesDeletionStore>(scope.ServiceProvider)
            .ClaimAsync(max, token, options.Value.Lease, options.Value.MaxAttempts, ct);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "存储删除失败时记录日志并退避重试，避免单个文件的失败中断队列处理")]
    public async Task ProcessAsync(DeletionItem item, Guid token, CancellationToken ct)
    {
        try
        {
            await storage.DeleteAsync(item.StorageKey, ct);
        }
        catch (Exception exception) when (!ct.IsCancellationRequested)
        {
            FilesLog.ObjectDeletionFailed(logger, exception, item.Id);
            await RecordAsync(store => store.FailAsync(item, token, ct), ct);
            return;
        }

        await RecordAsync(store => store.CompleteAsync(item, token, ct), ct);
    }

    private async Task RecordAsync(Func<FilesDeletionStore, Task> write, CancellationToken ct)
    {
        await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, CurrentUser.System());
        ct.ThrowIfCancellationRequested();
        await write(ActivatorUtilities.CreateInstance<FilesDeletionStore>(scope.ServiceProvider));
    }
}
