using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 查询存储问题：对账发现库里有记录而存储里找不到对象的文件，待运维还原
/// </summary>
/// <param name="Status">状态编码，可以给多个</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(FilesPermissions.FileView)]
public sealed record ListStorageIssuesQuery(IReadOnlyList<string>? Status, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<StorageIssueItem>>;
