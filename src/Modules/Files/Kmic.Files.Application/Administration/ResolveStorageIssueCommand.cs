using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 把存储问题标为已解决
/// </summary>
/// <remarks>
/// 运维按运维手册把对象还原之后用。已经解决的再来一次什么也不改，算成功
/// </remarks>
/// <param name="Id">存储问题</param>
/// <param name="Note">处理说明，必填，如还原的工单号</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(FilesPermissions.FileManage)]
[Audited]
[Idempotent]
public sealed record ResolveStorageIssueCommand(StorageIssueId Id, string Note, long ExpectedVersion) : ICommand<VersionedResponse<StorageIssueId>>;
