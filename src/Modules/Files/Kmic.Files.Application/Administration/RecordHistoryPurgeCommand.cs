using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 登记运维已经在对象存储里彻底删除了一个文件的历史版本
/// </summary>
/// <remarks>
/// 应用的存储账号没有删除指定版本的权限，历史版本由运维另行清除，这里只留执行记录。
/// 只有已经彻底删除的文件可以登记；在登记之前，内容只存在于非当前版本里，应用不提供任何读取路径
/// </remarks>
/// <param name="Id">文件</param>
/// <param name="Note">执行记录，如工单号，必填</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(FilesPermissions.FilePurge)]
[Audited]
public sealed record RecordHistoryPurgeCommand(StoredFileId Id, string Note, long ExpectedVersion) : ICommand<VersionedResponse<StoredFileId>>;
