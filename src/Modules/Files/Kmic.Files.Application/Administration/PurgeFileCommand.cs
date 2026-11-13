using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 彻底删除一个文件：对象由删除工作器删除（存储里只留删除标记），元数据留下记录，引用它的单据显示“文件已删除”
/// </summary>
/// <remarks>
/// 不可恢复。误传了敏感文件时用；历史版本由运维在对象存储里另行清除，之后用 RecordHistoryPurge 登记。
/// 已经彻底删除或正在删除的文件再来一次什么也不改，算成功。业务正在关联这个文件时返回暂时冲突，稍后再试
/// </remarks>
/// <param name="Id">文件</param>
/// <param name="Reason">原因，必填，写进文件的记录</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(FilesPermissions.FilePurge)]
[Audited]
[Idempotent]
public sealed record PurgeFileCommand(StoredFileId Id, string Reason, long ExpectedVersion) : ICommand<VersionedResponse<StoredFileId>>;
