using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 查询存储问题列表
/// </summary>
public sealed class ListStorageIssuesQueryHandler(IFilesQueryDb db) : IQueryHandler<ListStorageIssuesQuery, PagedResult<StorageIssueItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<StorageIssueItem>>> Handle(ListStorageIssuesQuery query, CancellationToken cancellationToken)
    {
        var statuses = EnumerationFilter.Parse<StorageIssueStatus>(query.Status, "status");
        if (statuses.IsFailure)
        {
            return statuses.Error;
        }

        var issues = db.StorageIssues.AsQueryable();
        var wanted = statuses.Value;
        if (wanted.Count > 0)
        {
            issues = issues.Where(i => wanted.Contains(i.Status));
        }

        var paged = await issues
            .Select(i => new StorageIssueItem(i.Id, i.FileId, null, i.Kind, i.ObjectVersionId, i.FoundAt, i.Status, i.ResolvedAt, i.ResolvedBy, i.ResolutionNote, i.ConcurrencyVersion))
            .ToPagedAsync(query.Page, query.Sort, StorageIssueSort.Map, cancellationToken);
        if (paged.IsFailure)
        {
            return paged.Error;
        }

        var page = paged.Value;

        // 文件记录不存在时仍返回问题记录，文件名为空
        var fileIds = page.Items.Select(i => i.FileId).Distinct().ToList();
        var names = await db.Files
            .Where(f => fileIds.Contains(f.Id))
            .Select(f => new { f.Id, f.OriginalName })
            .ToDictionaryAsync(f => f.Id, f => f.OriginalName, cancellationToken);
        return page with { Items = [.. page.Items.Select(i => i with { FileName = names.GetValueOrDefault(i.FileId) })] };
    }
}
