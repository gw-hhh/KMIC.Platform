using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 让删除失败太多次的文件重新进入删除队列
/// </summary>
/// <remarks>
/// 删除工作器对一个文件尝试达到上限后不再自动领取，留在队列里等人处理：先排除存储的问题，再用这个命令清零尝试次数
/// </remarks>
/// <param name="Id">文件</param>
[RequirePermission(FilesPermissions.FileManage)]
[Audited]
public sealed record RetryDeletionCommand(StoredFileId Id) : ICommand;
