using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Attachments;

public sealed class RegisterGeneratedFileCommandHandler(IStoredFileRepository files, TimeProvider time) : ICommandHandler<RegisterGeneratedFileCommand>
{
    public async ValueTask<Result> Handle(RegisterGeneratedFileCommand command, CancellationToken cancellationToken)
    {
        var message = command.Message;
        var id = StoredFileId.From(message.FileId);
        if (await files.ExistsAsync(id, cancellationToken))
        {
            return Result.Success();
        }

        files.Add(StoredFile.RegisterGenerated(
            id,
            message.StorageKey,
            message.ObjectVersionId,
            message.OriginalName,
            message.FileType,
            message.ContentType,
            message.Size,
            message.Sha256,
            message.Source,
            message.RequestedBy is { } user ? UserId.From(user) : null,
            time.GetUtcNow(),
            message.ExpiresAt));
        return Result.Success();
    }
}
