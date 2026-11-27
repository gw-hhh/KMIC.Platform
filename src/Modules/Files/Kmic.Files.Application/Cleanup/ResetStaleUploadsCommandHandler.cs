using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Cleanup;

public sealed class ResetStaleUploadsCommandHandler(IStoredFileRepository files, IOptions<FilesOptions> options, TimeProvider time)
    : ICommandHandler<ResetStaleUploadsCommand, ScheduledJobOutcome>
{
    private const string Reason = "上传超时未完成";

    public async ValueTask<Result<ScheduledJobOutcome>> Handle(ResetStaleUploadsCommand command, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var stale = await files.FindStaleUploadsAsync(now - options.Value.StaleUpload, options.Value.PurgeBatchSize, cancellationToken);
        var rejected = 0;
        foreach (var id in stale)
        {
            // 候选状态可能已变化；重读后仅拒绝仍在上传或扫描的文件
            var loaded = await files.GetForUpdateAsync(id, null, cancellationToken);
            if (loaded.IsSuccess && loaded.Value.Reject(Reason, now, options.Value.RejectedRetention).IsSuccess)
            {
                rejected++;
            }
        }

        return new ScheduledJobOutcome(rejected);
    }
}
