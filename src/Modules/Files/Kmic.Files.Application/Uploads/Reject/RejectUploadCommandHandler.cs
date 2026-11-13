using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Uploads.Reject;

/// <summary>
/// 处理 <see cref="RejectUploadCommand"/>
/// </summary>
public sealed class RejectUploadCommandHandler(IStoredFileRepository files, IExecutionContext execution, IOptions<FilesOptions> options, TimeProvider time)
    : ICommandHandler<RejectUploadCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RejectUploadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await files.GetForUpdateAsync(StoredFileId.From(command.FileId), null, cancellationToken);
        if (loaded.IsFailure || loaded.Value.UploadedBy != execution.User.UserId)
        {
            return FilesErrors.NotFound;
        }

        return loaded.Value.Reject(command.Reason.Trim(), time.GetUtcNow(), options.Value.RejectedRetention);
    }
}
