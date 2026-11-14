using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Routes;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class FinalizeRouteEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/routes/finalize", HandleAsync)
            .Sends<FinalizeRouteCommand>()
            .WithSummary("终结订阅")
            .WithDescription("只有待终结的订阅能终结，之后不再向它投递，旧构建不能自动恢复它，要在受控的维护窗口里做。还有运行中的实例在声明它、有消息正在处理时拒绝。有没处理完的消息时，选择放弃（原因连同原来的错误一起记下）才能终结。必须带幂等键。");

    private static async Task<IResult> HandleAsync(FinalizeRouteRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new FinalizeRouteCommand(request.MessageType, request.Subscription, request.Reason, request.AbandonUnfinished), ct)).ToOk();
}
