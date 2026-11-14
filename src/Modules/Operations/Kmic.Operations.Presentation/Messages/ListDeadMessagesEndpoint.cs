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
            .WithDescription("汇总各模块收件箱和发件箱的死信，按发生时间倒序返回。使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。列表不含消息内容，需通过详情接口查看。");

    private static async Task<IResult> HandleAsync([AsParameters] ListDeadMessagesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListDeadMessagesQuery(request.Module, request.Box, request.Subscription, request.MessageType, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
