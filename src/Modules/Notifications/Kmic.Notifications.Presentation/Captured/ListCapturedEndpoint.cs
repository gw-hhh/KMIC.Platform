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
            .WithDescription("分页查询捕获模式下保存的邮件与短信摘要（不含正文，记录读取审计）；未开启捕获返回 404");

    private static async Task<IResult> HandleAsync([AsParameters] ListCapturedRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListCapturedQuery(request.Channel, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}
