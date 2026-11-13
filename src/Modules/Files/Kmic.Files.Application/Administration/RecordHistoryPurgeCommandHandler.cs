using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 处理 <see cref="RecordHistoryPurgeCommand"/>
/// </summary>
public sealed class RecordHistoryPurgeCommandHandler(IStoredFileRepository files, TimeProvider time)
    : ICommandHandler<RecordHistoryPurgeCommand, VersionedResponse<StoredFileId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<StoredFileId>>> Handle(RecordHistoryPurgeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await files.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var recorded = loaded.Value.RecordHistoryPurge(command.Note.Trim(), time.GetUtcNow());
        return recorded.IsFailure ? recorded.Error : new VersionedResponse<StoredFileId>(loaded.Value.Id);
    }
}
