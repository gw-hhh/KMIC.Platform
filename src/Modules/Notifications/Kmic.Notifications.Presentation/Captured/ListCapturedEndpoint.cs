using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Captured.List;

namespace Kmic.Notifications.Presentation.Captured;

internal sealed class ListCapturedEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/captured", HandleAsync)
            .Sends<ListCapturedQuery>()
            .Sortable(CapturedSort.Map)
            .WithSummary("查询捕获的通知内容")
            .WithDescription("查询捕获模式下保存的邮件和短信，不含正文。未开启捕获时返回 404。内容可能含验证码，查看会记录读审计。");

    private static async Task<IResult> HandleAsync([AsParameters] ListCapturedRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListCapturedQuery(request.Channel, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}
