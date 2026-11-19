using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 重置文件删除重试次数，排除存储故障后用
/// </summary>
[RequirePermission(FilesPermissions.FileManage)]
[Audited]
public sealed record RetryDeletionCommand(StoredFileId Id) : ICommand;
