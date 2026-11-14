using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Rebuilds;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class ListRebuildsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/messages/rebuilds", HandleAsync)
            .Sends<ListRebuildsQuery>()
            .WithSummary("查询读模型重建")
            .WithDescription("每个订阅最近几轮重建，新的在前。状态为等待确认的一轮，源端找不到的行太多而暂停了，要运维核实后确认收尾或放弃。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListRebuildsQuery(), ct)).ToOk();
}
