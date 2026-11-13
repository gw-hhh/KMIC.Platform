using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Background;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Infrastructure.Deletion;

/// <summary>
/// 删除队列：领取“删除中”的文件，在事务之外先删对象、再删行
/// </summary>
/// <remarks>
/// 先删对象再删行：中途失败最多留下一个可重试的“删除中”行，不会出现“库里有行、对象已经没了”。
/// 删除失败的按尝试次数退避；尝试次数用完的留在队列里，等人工在管理页面上重试。
/// 已彻底删除的文件也走这里：只删对象、行保留。对象不存在不算失败（删除是幂等的）。
/// 领取循环由框架提供，这里只写怎样领取、怎样处理一条
/// </remarks>
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

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "存储的任何失败都按“这次没删掉”处理：记日志、退避、下次重试，不能让这一条影响别的")]
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
