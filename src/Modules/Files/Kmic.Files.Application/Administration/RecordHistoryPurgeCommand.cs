using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 登记文件历史版本已由运维清除
/// </summary>
/// <remarks>
/// 应用存储账号无权删除指定版本；运维须先清除历史版本，再通过此命令登记。
/// 仅已彻底删除的文件可登记；文件标记为已彻底删除后，应用不再提供内容读取
/// </remarks>
/// <param name="Id">文件 ID</param>
/// <param name="Note">执行记录，如工单号，必填</param>
/// <param name="ExpectedVersion">预期版本号，与当前版本不符时返回版本冲突</param>
[RequirePermission(FilesPermissions.FilePurge)]
[Audited]
public sealed record RecordHistoryPurgeCommand(StoredFileId Id, string Note, long ExpectedVersion) : ICommand<VersionedResponse<StoredFileId>>;
