using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Uploads.Complete;

/// <summary>
/// 处理 <see cref="CompleteUploadCommand"/>
/// </summary>
public sealed class CompleteUploadCommandHandler(IStoredFileRepository files, IExecutionContext execution, IOptions<FilesOptions> options, TimeProvider time)
    : ICommandHandler<CompleteUploadCommand, UploadCompleted>
{
    /// <inheritdoc />
    public async ValueTask<Result<UploadCompleted>> Handle(CompleteUploadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await files.GetForUpdateAsync(StoredFileId.From(command.FileId), null, cancellationToken);

        // 别人的文件和不存在的文件一个样，不让人探测别人的文件 ID
        if (loaded.IsFailure || loaded.Value.UploadedBy != execution.User.UserId)
        {
            return FilesErrors.NotFound;
        }

        var file = loaded.Value;
        var completed = file.CompleteUpload(command.FileType, command.ContentType, command.Size, command.Sha256, command.ObjectVersionId, needsScan: false, time.GetUtcNow(), options.Value.TempExpiry);
        if (completed.IsFailure)
        {
            return completed.Error;
        }

        return new UploadCompleted(file.Id.Value, file.OriginalName, command.Size, command.FileType, file.ExpiresAt!.Value);
    }
}
