using Mediator;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

public static class EndpointConventionExtensions
{
    /// <summary>声明当前端点发送的命令或查询消息（每个端点仅允许声明一个）</summary>
    public static RouteHandlerBuilder Sends<TMessage>(this RouteHandlerBuilder builder)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(builder);

        var (status, body) = MessageResponse.Of(typeof(TMessage));
        return builder
            .WithMetadata(new SendsMetadata(typeof(TMessage)))
            .Produces(status, body, body is null ? null : "application/json");
    }
}
