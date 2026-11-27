using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.SecurityEvents;

namespace Kmic.Identity.Presentation.SecurityEvents;

internal sealed class ListSecurityEventsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/security-events", HandleAsync)
            .Sends<ListSecurityEventsQuery>()
            .Sortable(SecurityEventSort.Map)
            .WithSummary("查询安全事件")
            .WithDescription("分页查询登录失败、越权、授权变更与模拟等安全事件；时间范围必填（最长 31 天，默认倒序），查询记录读取审计");

    private static async Task<IResult> HandleAsync([AsParameters] ListSecurityEventsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListSecurityEventsQuery(request.From, request.To, request.Type, request.UserId, request.ClientIp, request.Account, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}
