using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 列表分页
/// </summary>
public static class PagedQueryableExtensions
{
    /// <summary>
    /// 排序、计数、分页，精确计数
    /// </summary>
    public static Task<Result<PagedResult<T>>> ToPagedAsync<T>(
        this IQueryable<T> query, PageRequest page, SortRequest sort, SortMap<T> sortMap, CancellationToken ct) =>
        query.ToPagedAsync(page, sort, sortMap, CountMode.Exact, ct);

    /// <summary>
    /// 排序、计数、分页
    /// </summary>
    public static async Task<Result<PagedResult<T>>> ToPagedAsync<T>(
        this IQueryable<T> query, PageRequest page, SortRequest sort, SortMap<T> sortMap, CountMode count, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(sort);
        ArgumentNullException.ThrowIfNull(sortMap);

        var terms = sortMap.Parse(sort.Sort);
        if (Validate(page, terms) is { Count: > 0 } problems)
        {
            return CommonErrors.ValidationFailedWith(problems);
        }

        long skip = ((long)page.Page - 1) * page.PageSize;
        var (total, exact) = await CountAsync(query, count, ct);
        if (!exact && (long)page.Page * page.PageSize > count.Cap)
        {
            return CommonErrors.PageOutOfRange;
        }

        IReadOnlyList<T> items = skip >= total
            ? []
            : await sortMap.Apply(query, terms.Value).Skip((int)skip).Take(page.PageSize).ToListAsync(ct);
        return new PagedResult<T>(items, page.Page, page.PageSize, total, exact);
    }

    private static List<FieldError> Validate(PageRequest page, Result<SortTerm[]> terms)
    {
        var problems = new List<FieldError>();
        if (page.Page < 1)
        {
            problems.Add(new FieldError("page", "Range", "页码必须大于等于 1"));
        }

        if (page.PageSize is < 1 or > PageRequest.MaxPageSize)
        {
            problems.Add(new FieldError("pageSize", "Range", $"每页数量必须在 1 到 {PageRequest.MaxPageSize} 之间"));
        }

        if (terms.IsFailure)
        {
            problems.AddRange(terms.Error.FieldErrors);
        }

        return problems;
    }

    private static async Task<(long Total, bool Exact)> CountAsync<T>(IQueryable<T> query, CountMode count, CancellationToken ct)
    {
        if (!count.IsCapped)
        {
            return (await query.LongCountAsync(ct), true);
        }

        // 多数一行，就能知道有没有超过上限，而不用数完全部
        var counted = await query.Take(count.Cap + 1).CountAsync(ct);
        return counted > count.Cap ? (count.Cap, false) : (counted, true);
    }
}
