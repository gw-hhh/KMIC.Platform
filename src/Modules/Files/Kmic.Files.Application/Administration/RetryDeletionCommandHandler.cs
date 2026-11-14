using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 重置文件删除重试次数
/// </summary>
public sealed class RetryDeletionCommandHandler(IStoredFileRepository files) : ICommandHandler<RetryDeletionCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RetryDeletionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 文件不存在或不在删除队列中，均返回未找到
        return await files.ResetDeletionAsync(command.Id, cancellationToken) ? Result.Success() : FilesErrors.NotFound;
    }
}
