using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Cleanup;

/// <summary>
/// 将超时未完成的上传标记为被拒绝
/// </summary>
public sealed class ResetStaleUploadsCommandHandler(IStoredFileRepository files, IOptions<FilesOptions> options, TimeProvider time)
    : ICommandHandler<ResetStaleUploadsCommand, ScheduledJobOutcome>
{
    private const string Reason = "上传超时未完成";

    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(ResetStaleUploadsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = time.GetUtcNow();
        var stale = await files.FindStaleUploadsAsync(now - options.Value.StaleUpload, options.Value.PurgeBatchSize, cancellationToken);
        var rejected = 0;
        foreach (var id in stale)
        {
            // 上传可能已完成，重读后由 Reject 校验是否仍为上传中
            var loaded = await files.GetForUpdateAsync(id, null, cancellationToken);
            if (loaded.IsSuccess && loaded.Value.Reject(Reason, now, options.Value.RejectedRetention).IsSuccess)
            {
                rejected++;
            }
        }

        return new ScheduledJobOutcome(rejected);
    }
}
