using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Rebuilds;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class ConfirmRebuildEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/rebuilds/confirm", HandleAsync)
            .Sends<ConfirmRebuildCommand>()
            .WithSummary("确认读模型重建收尾")
            .WithDescription("运维核实缺失行在源端确实不存在后确认收尾，调度后台作业置终态；无待确认任务返回 409");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ConfirmRebuildCommand(request.Subscription), ct)).ToNoContent();
}
