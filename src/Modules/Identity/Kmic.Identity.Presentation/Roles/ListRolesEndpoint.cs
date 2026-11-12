using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.List;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class ListRolesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/roles", HandleAsync)
            .Sends<ListRolesQuery>()
            .Sortable(RoleSort.Map)
            .WithSummary("查询角色列表")
            .WithDescription("角色、状态、是否受保护和持有人数，分页。");

    private static async Task<IResult> HandleAsync([AsParameters] ListRolesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListRolesQuery(request.Q, request.Status, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}
