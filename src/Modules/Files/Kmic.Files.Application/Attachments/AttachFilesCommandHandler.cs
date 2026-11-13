using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// 处理 <see cref="AttachFilesCommand"/>
/// </summary>
public sealed class AttachFilesCommandHandler(IStoredFileRepository files, TimeProvider time) : ICommandHandler<AttachFilesCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(AttachFilesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var message = command.Message;
        var now = time.GetUtcNow();
        foreach (var id in message.FileIds.Distinct())
        {
            var loaded = await files.GetForUpdateAsync(StoredFileId.From(id), null, cancellationToken);
            if (loaded.IsFailure)
            {
                return loaded.Error;
            }

            var attached = loaded.Value.Attach(message.OwnerModule, message.OwnerType, message.OwnerId, now);
            if (attached.IsFailure)
            {
                return attached.Error;
            }
        }

        return Result.Success();
    }
}
