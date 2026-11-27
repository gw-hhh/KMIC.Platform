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
            .WithDescription("汇总各重建模块的最近 20 条记录（按开始时间倒序）；“等待确认”表示缺失行超限，须运维核实后确认或放弃");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListRebuildsQuery(), ct)).ToOk();
}
