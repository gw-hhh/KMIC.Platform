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
            .WithDescription("返回各模块发件箱的积压数、最早积压消息的等待时长和死信数，以及收件箱各订阅的积压、死信和阻塞顺序键数量。isRetiring 标记待终结订阅，其积压属预期情况，应与普通订阅分开告警。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetMessageSummaryQuery(), ct)).ToOk();
}
