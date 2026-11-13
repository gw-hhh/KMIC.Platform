using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 处理 <see cref="ResetStaleUploadsCommand"/>
/// </summary>
public sealed class ResetStaleUploadsCommandHandler(IStoredFileRepository files, IOptions<FilesOptions> options, TimeProvider time)
    : ICommandHandler<ResetStaleUploadsCommand, ScheduledJobOutcome>
{
    private const string Reason = "上传中断";

    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(ResetStaleUploadsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = time.GetUtcNow();
        var stale = await files.FindStaleUploadsAsync(now - options.Value.StaleUpload, options.Value.PurgeBatchSize, cancellationToken);
        var rejected = 0;
        foreach (var id in stale)
        {
            // 读到的现在已经不是“上传中”的（上传刚好完成）就不动
            var loaded = await files.GetForUpdateAsync(id, null, cancellationToken);
            if (loaded.IsSuccess && loaded.Value.Reject(Reason, now, options.Value.RejectedRetention).IsSuccess)
            {
                rejected++;
            }
        }

        return new ScheduledJobOutcome(rejected);
    }
}
