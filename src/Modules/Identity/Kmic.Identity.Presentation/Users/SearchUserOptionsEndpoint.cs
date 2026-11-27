using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Options;

namespace Kmic.Identity.Presentation.Users;

internal sealed class SearchUserOptionsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/user-options", HandleAsync)
            .Sends<SearchUserOptionsQuery>()
            .WithSummary("查询用户选择列表")
            .WithDescription("按姓名分页返回用户选项（ID、姓名、工号、所属组织及状态，不含手机号与邮箱）");

    private static async Task<IResult> HandleAsync([AsParameters] SearchUserOptionsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SearchUserOptionsQuery(request.Q, new PageRequest(request.Page, request.PageSize)), ct)).ToOk();
}
