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
            .WithSummary("选人下拉")
            .WithDescription("登录即可用。只返回姓名、工号、所属组织和状态，不含手机号和邮箱；按姓名排序，分页。");

    private static async Task<IResult> HandleAsync([AsParameters] SearchUserOptionsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SearchUserOptionsQuery(request.Q, new PageRequest(request.Page, request.PageSize)), ct)).ToOk();
}
