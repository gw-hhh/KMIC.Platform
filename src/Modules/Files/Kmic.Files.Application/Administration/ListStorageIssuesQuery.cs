using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 查询存储问题列表
/// </summary>
/// <param name="Status">状态编码</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(FilesPermissions.FileView)]
public sealed record ListStorageIssuesQuery(IReadOnlyList<string>? Status, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<StorageIssueItem>>;
