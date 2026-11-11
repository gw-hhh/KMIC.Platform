using Mediator;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 端点声明的扩展方法
/// </summary>
public static class EndpointConventionExtensions
{
    /// <summary>
    /// 声明本端点发送的命令或查询
    /// </summary>
    /// <remarks>
    /// 启动时据此对照用例目录校验，并生成 OpenAPI 里的权限、幂等等说明，成功响应的状态码与类型也按消息声明。
    /// 一个端点发送且只发送一个消息
    /// </remarks>
    /// <typeparam name="TMessage">命令或查询的类型</typeparam>
    /// <param name="builder">端点</param>
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
