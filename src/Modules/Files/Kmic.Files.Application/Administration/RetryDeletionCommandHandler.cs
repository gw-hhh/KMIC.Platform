using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 处理 <see cref="RetryDeletionCommand"/>
/// </summary>
public sealed class RetryDeletionCommandHandler(IStoredFileRepository files) : ICommandHandler<RetryDeletionCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RetryDeletionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 不在删除队列里（文件不存在，或没有要删的）：对管理员来说都是“没有可重试的”
        return await files.ResetDeletionAsync(command.Id, cancellationToken) ? Result.Success() : FilesErrors.NotFound;
    }
}
