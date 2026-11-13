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
            .WithDescription("非生产环境不真的发出邮件和短信，内容留在这里供测试人员查看，不含正文。没有开启捕获（生产环境）时返回 404。内容可能含验证码，查看会记读审计。");

    private static async Task<IResult> HandleAsync([AsParameters] ListCapturedRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListCapturedQuery(request.Channel, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}
