using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Application.Uploads.Complete;

public sealed class CompleteUploadCommandHandler(IStoredFileRepository files, IExecutionContext execution, IOptions<FilesOptions> options, TimeProvider time)
    : ICommandHandler<CompleteUploadCommand, UploadCompleted>
{
    public async ValueTask<Result<UploadCompleted>> Handle(CompleteUploadCommand command, CancellationToken cancellationToken)
    {
        var loaded = await files.GetForUpdateAsync(StoredFileId.From(command.FileId), null, cancellationToken);

        // 无权访问和不存在的文件统一返回 NotFound，避免泄露文件是否存在
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
