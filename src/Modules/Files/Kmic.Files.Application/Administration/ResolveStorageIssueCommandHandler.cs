using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

public sealed class ResolveStorageIssueCommandHandler(IStorageIssueRepository issues, IExecutionContext execution, TimeProvider time)
    : ICommandHandler<ResolveStorageIssueCommand, VersionedResponse<StorageIssueId>>
{
    public async ValueTask<Result<VersionedResponse<StorageIssueId>>> Handle(ResolveStorageIssueCommand command, CancellationToken cancellationToken)
    {
        var loaded = await issues.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var by = execution.User.ActualUserId ?? execution.User.UserId ?? UserId.System;
        var resolved = loaded.Value.Resolve(command.Note.Trim(), by, time.GetUtcNow());
        return resolved.IsFailure ? resolved.Error : new VersionedResponse<StorageIssueId>(loaded.Value.Id);
    }
}
