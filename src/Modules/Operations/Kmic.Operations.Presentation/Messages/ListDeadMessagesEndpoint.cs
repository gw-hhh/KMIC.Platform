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
            .WithDescription("汇总各模块收发件箱死信（不含正文），按发生时间倒序游标分页（传 nextCursor 到 after）");

    private static async Task<IResult> HandleAsync([AsParameters] ListDeadMessagesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListDeadMessagesQuery(request.Module, request.Box, request.Subscription, request.MessageType, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
