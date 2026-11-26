using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Maintenance.SpecimenCategories;

namespace Kmic.Sample.Presentation.SpecimenCategories;

internal sealed class ListSpecimenCategoriesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/specimen-categories", HandleAsync)
            .Sends<ListSpecimenCategoriesQuery>()
            .Sortable(SpecimenCategorySort.Map)
            .WithSummary("查询示例分类列表")
            .WithDescription("包括已停用的分类。每页条数须为 1 到 200，否则返回 400。");

    private static async Task<IResult> HandleAsync([AsParameters] ListSpecimenCategoriesRequest request, IQuerySender sender, CancellationToken ct)
    {
        var query = new ListSpecimenCategoriesQuery(request.Q, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort));
        return (await sender.SendAsync(query, ct)).ToOk();
    }
}
