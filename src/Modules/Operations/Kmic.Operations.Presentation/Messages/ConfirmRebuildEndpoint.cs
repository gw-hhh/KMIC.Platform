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
            .WithDescription("源端找不到的行太多而暂停的一轮，运维核实它们确实没有了之后确认：另排一个作业把这些行置为终态并完成这一轮。没有暂停的一轮返回 409。成功返回 204。必须带幂等键。");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ConfirmRebuildCommand(request.Subscription), ct)).ToNoContent();
}
