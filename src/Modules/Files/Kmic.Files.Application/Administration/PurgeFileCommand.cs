using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 彻底删除文件并保留元数据记录，存储对象由删除工作器处理
/// </summary>
/// <remarks>
/// 用于清除误传的敏感文件，操作不可恢复，引用方显示“文件已删除”。历史版本须由运维另行清除并通过 RecordHistoryPurge 登记。
/// 版本校验通过后，已彻底删除或正在删除的文件重复操作直接成功；存在并发关联或未完成的关联请求时返回暂时冲突，可稍后重试
/// </remarks>
/// <param name="Id">文件 ID</param>
/// <param name="Reason">删除原因，必填，保存到文件记录</param>
/// <param name="ExpectedVersion">预期版本号，与当前版本不符时返回版本冲突</param>
[RequirePermission(FilesPermissions.FilePurge)]
[Audited]
[Idempotent]
public sealed record PurgeFileCommand(StoredFileId Id, string Reason, long ExpectedVersion) : ICommand<VersionedResponse<StoredFileId>>;
