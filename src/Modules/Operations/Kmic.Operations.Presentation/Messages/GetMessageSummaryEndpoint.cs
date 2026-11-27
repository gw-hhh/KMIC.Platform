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
            .WithSummary("查询各模块的消息积压")
            .WithDescription("汇总发件箱与收件箱各订阅的积压、死信及阻塞顺序键数；isRetiring 订阅积压属预期情况，应与普通订阅分开告警");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetMessageSummaryQuery(), ct)).ToOk();
}
