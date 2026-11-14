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
            .WithDescription("仅可终结待终结的订阅。操作须在维护窗口执行；终结后停止投递，旧构建不能自动恢复订阅。仍有心跳未过期的实例声明该订阅，或消息的处理占用尚未到期时，拒绝终结。有未完成消息时，须选择放弃，放弃原因与原错误一并记录。必须提供幂等键。");

    private static async Task<IResult> HandleAsync(FinalizeRouteRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new FinalizeRouteCommand(request.MessageType, request.Subscription, request.Reason, request.AbandonUnfinished), ct)).ToOk();
}
