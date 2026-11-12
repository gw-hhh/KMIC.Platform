using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.List;

namespace Kmic.Identity.Presentation.Users;

internal sealed class ListUsersEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/users", HandleAsync)
            .Sends<ListUsersQuery>()
            .Sortable(UserSort.Map)
            .WithSummary("查询用户列表")
            .WithDescription("账号、姓名、工号、手机号、状态、所属组织和拥有的角色，分页。系统账号不在列表里。");

    private static async Task<IResult> HandleAsync([AsParameters] ListUsersRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListUsersQuery(request.Q, request.Status, request.RoleId, request.OrgUnitId, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}
