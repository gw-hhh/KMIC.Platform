using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Uploads.Start;

/// <summary>
/// 处理 <see cref="StartUploadCommand"/>
/// </summary>
public sealed class StartUploadCommandHandler(
    UploadProfileCatalog profiles,
    IResourceAuthorizer<UploadProfileTarget, UploadAction> authorizer,
    IStoredFileRepository files,
    IStorageKeyFactory keys,
    IExecutionContext execution,
    TimeProvider time) : ICommandHandler<StartUploadCommand, UploadStarted>
{
    /// <inheritdoc />
    public ValueTask<Result<UploadStarted>> Handle(StartUploadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (profiles.Find(command.Profile) is not { } profile)
        {
            return ValueTask.FromResult<Result<UploadStarted>>(FilesErrors.ProfileNotFound);
        }

        var allowed = authorizer.Check(new UploadProfileTarget(profile.Name, profile.Permission), UploadAction.Upload);
        if (allowed.IsFailure)
        {
            return ValueTask.FromResult<Result<UploadStarted>>(allowed.Error);
        }

        if (command.DeclaredSize > profile.MaxBytes)
        {
            return ValueTask.FromResult<Result<UploadStarted>>(FilesErrors.TooLarge);
        }

        if (FileNames.Sanitize(command.FileName) is not { } name)
        {
            return ValueTask.FromResult<Result<UploadStarted>>(CommonErrors.ValidationFailedWith([new FieldError("fileName", "Invalid", "文件名不能是空的")]));
        }

        var now = time.GetUtcNow();
        var id = StoredFileId.New();
        var key = keys.KeyFor(id.Value, now);
        files.Add(StoredFile.Start(id, key, name, profile.Name, execution.User.UserId, now));
        return ValueTask.FromResult<Result<UploadStarted>>(new UploadStarted(id.Value, key, name));
    }
}
