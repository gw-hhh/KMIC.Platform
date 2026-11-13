using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Dead;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class ListDeadMessagesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/messages/dead", HandleAsync)
            .Sends<ListDeadMessagesQuery>()
            .WithSummary("查询各模块的死信")
            .WithDescription("遍历每个模块自己的发件箱与收件箱，按发生时间倒序归并，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。只列死信，不含载荷；载荷用详情接口看。");

    private static async Task<IResult> HandleAsync([AsParameters] ListDeadMessagesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListDeadMessagesQuery(request.Module, request.Box, request.Subscription, request.MessageType, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
