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
            .WithDescription("运维核实缺失行在源端确实不存在后，确认等待收尾的重建。系统另排作业，将这些行置为终态并完成重建。没有等待确认的重建时返回 409；");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ConfirmRebuildCommand(request.Subscription), ct)).ToNoContent();
}
