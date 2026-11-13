using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Summary;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class GetMessageSummaryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/messages/summary", HandleAsync)
            .Sends<GetMessageSummaryQuery>()
            .WithSummary("各模块的消息积压汇总")
            .WithDescription("每个模块的发件箱积压、最老一条的年龄、死信数，以及收件箱里每个订阅的积压、死信、被挡住的顺序键。待终结订阅的积压是预期内的，带 isRetiring 标记，不要和普通订阅的积压一起告警。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetMessageSummaryQuery(), ct)).ToOk();
}
