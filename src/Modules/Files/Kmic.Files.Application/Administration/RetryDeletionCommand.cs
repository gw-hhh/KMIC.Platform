using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 重置文件删除重试次数
/// </summary>
/// <remarks>
/// 删除次数达到上限后停止自动重试，文件仍保留在队列中。排除存储故障后可用此命令清零次数；仅删除队列中的文件可重试
/// </remarks>
[RequirePermission(FilesPermissions.FileManage)]
[Audited]
public sealed record RetryDeletionCommand(StoredFileId Id) : ICommand;
