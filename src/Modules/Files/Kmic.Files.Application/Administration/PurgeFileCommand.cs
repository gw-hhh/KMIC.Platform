using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>彻底删除文件并保留元数据记录（不可恢复），存储对象由删除队列异步清理</summary>
/// <param name="Id">文件 ID</param>
/// <param name="Reason">删除原因</param>
/// <param name="ExpectedVersion">预期版本号</param>
[RequirePermission(FilesPermissions.FilePurge)]
[Audited]
[Idempotent]
public sealed record PurgeFileCommand(StoredFileId Id, string Reason, long ExpectedVersion) : ICommand<VersionedResponse<StoredFileId>>;
