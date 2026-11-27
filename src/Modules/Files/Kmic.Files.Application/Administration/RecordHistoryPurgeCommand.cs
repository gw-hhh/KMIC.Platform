using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>登记文件历史版本已由运维清除（须先清除再登记）</summary>
/// <param name="Id">文件 ID</param>
/// <param name="Note">执行记录</param>
/// <param name="ExpectedVersion">预期版本号</param>
[RequirePermission(FilesPermissions.FilePurge)]
[Audited]
public sealed record RecordHistoryPurgeCommand(StoredFileId Id, string Note, long ExpectedVersion) : ICommand<VersionedResponse<StoredFileId>>;
