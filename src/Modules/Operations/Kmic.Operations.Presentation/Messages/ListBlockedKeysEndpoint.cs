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
            .WithSummary("被有序订阅里的死信挡住的顺序键")
            .WithDescription("同一个键下排在死信后面的消息都在等它：处理掉最早的那条死信（重试，或确认不需要而放弃）才能放开。每个模块最多 500 个键，等得最多的排前面。");

    private static async Task<IResult> HandleAsync([Description("只看这个模块的；不给就是全部")] string? module, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListBlockedKeysQuery(module), ct)).ToOk();
}
