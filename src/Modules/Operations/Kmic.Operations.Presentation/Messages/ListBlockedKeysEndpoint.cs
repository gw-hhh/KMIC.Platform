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
            .WithDescription("按等待消息数倒序返回被死信阻塞的顺序键（每模块最多 500 个）；须先重试或放弃最早死信才能放行后续消息");

    private static async Task<IResult> HandleAsync([Description("模块筛选，未指定时查询全部模块")] string? module, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListBlockedKeysQuery(module), ct)).ToOk();
}
