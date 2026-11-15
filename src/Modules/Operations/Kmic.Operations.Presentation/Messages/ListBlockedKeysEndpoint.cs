using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Blocked;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class ListBlockedKeysEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/messages/blocked-keys", HandleAsync)
            .Sends<ListBlockedKeysQuery>()
            .WithSummary("查询被死信阻塞的顺序键")
            .WithDescription("同一顺序键下的后续消息须等待最早的死信处理完成；可重试该死信；允许放弃且确认无需再处理时，也可放弃。每个模块最多返回 500 个键，按等待消息数倒序排列。");

    private static async Task<IResult> HandleAsync([Description("模块筛选，未指定时查询全部模块")] string? module, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListBlockedKeysQuery(module), ct)).ToOk();
}
