using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 把存储问题标为已解决
/// </summary>
/// <param name="Id">存储问题</param>
/// <param name="Note">处理说明</param>
/// <param name="ExpectedVersion">预期版本号</param>
[RequirePermission(FilesPermissions.FileManage)]
[Audited]
[Idempotent]
public sealed record ResolveStorageIssueCommand(StorageIssueId Id, string Note, long ExpectedVersion) : ICommand<VersionedResponse<StorageIssueId>>;
