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
            .WithDescription("从声明了读模型重建的模块中各取最近 20 条记录，合并后按开始时间倒序返回。等待确认表示缺失行数超限，须由运维核实后确认收尾或放弃。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListRebuildsQuery(), ct)).ToOk();
}
