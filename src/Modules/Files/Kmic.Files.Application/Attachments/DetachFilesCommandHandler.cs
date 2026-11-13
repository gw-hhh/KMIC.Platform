using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// 处理 <see cref="DetachFilesCommand"/>
/// </summary>
public sealed class DetachFilesCommandHandler(IStoredFileRepository files, IOptions<FilesOptions> options, TimeProvider time) : ICommandHandler<DetachFilesCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(DetachFilesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var message = command.Message;
        var now = time.GetUtcNow();
        foreach (var id in message.FileIds.Distinct())
        {
            var loaded = await files.GetForUpdateAsync(StoredFileId.From(id), null, cancellationToken);
            if (loaded.IsFailure)
            {
                continue;
            }

            var detached = loaded.Value.Detach(message.OwnerModule, message.OwnerType, message.OwnerId, now, options.Value.DetachedRetention);
            if (detached.IsFailure)
            {
                return detached.Error;
            }
        }

        return Result.Success();
    }
}
