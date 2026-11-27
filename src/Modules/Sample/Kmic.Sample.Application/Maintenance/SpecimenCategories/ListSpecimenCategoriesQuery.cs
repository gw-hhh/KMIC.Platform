using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.SpecimenCategories;

namespace Kmic.Sample.Application.Maintenance.SpecimenCategories;

/// <summary>查询示例分类列表</summary>
/// <param name="Q">名称关键字</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(SamplePermissions.View)]
public sealed record ListSpecimenCategoriesQuery(string? Q, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<SpecimenCategoryListItem>>;

/// <summary>示例分类列表项</summary>
/// <param name="Id">示例分类 ID</param>
/// <param name="Name">名称</param>
/// <param name="IsEnabled">是否启用</param>
/// <param name="Version">版本号，修改、删除时带上</param>
public sealed record SpecimenCategoryListItem(SpecimenCategoryId Id, string Name, bool IsEnabled, long Version);

public static class SpecimenCategorySort
{
    /// <summary>示例分类排序字段映射（默认按名称）</summary>
    public static readonly SortMap<SpecimenCategoryListItem> Map = SortMap<SpecimenCategoryListItem>
        .Field("name", x => x.Name)
        .Default("name");
}

public sealed class ListSpecimenCategoriesQueryHandler(ISampleQueryDb db)
    : IQueryHandler<ListSpecimenCategoriesQuery, PagedResult<SpecimenCategoryListItem>>
{
    public async ValueTask<Result<PagedResult<SpecimenCategoryListItem>>> Handle(ListSpecimenCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = db.SpecimenCategories;
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            categories = categories.Where(c => KmicFunctions.ContainsText(c.Name, query.Q));
        }

        return await categories
            .Select(c => new SpecimenCategoryListItem(c.Id, c.Name, c.IsEnabled, c.ConcurrencyVersion))
            .ToPagedAsync(query.Page, query.Sort, SpecimenCategorySort.Map, cancellationToken);
    }
}
