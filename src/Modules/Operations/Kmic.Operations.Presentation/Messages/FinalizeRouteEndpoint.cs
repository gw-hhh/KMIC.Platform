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
            .WithDescription("订阅须处于“待终结”状态。操作须在维护窗口执行；终结后停止投递，旧构建不能自动恢复订阅。仍有心跳未过期的实例声明该订阅，或消息处于“处理中”且占用未到期时，不能终结。未完成的消息须先处理完成，或在终结时选择放弃；放弃原因与原错误一并记录。必须提供幂等键。");

    private static async Task<IResult> HandleAsync(FinalizeRouteRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new FinalizeRouteCommand(request.MessageType, request.Subscription, request.Reason, request.AbandonUnfinished), ct)).ToOk();
}
