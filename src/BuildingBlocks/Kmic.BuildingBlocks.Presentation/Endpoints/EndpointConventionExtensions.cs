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
    /// 启动时据此对照用例目录校验，并生成 OpenAPI 里的权限、幂等等说明。一个端点发送且只发送一个消息
    /// </remarks>
    /// <typeparam name="TMessage">命令或查询的类型</typeparam>
    /// <param name="builder">端点</param>
    public static RouteHandlerBuilder Sends<TMessage>(this RouteHandlerBuilder builder)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithMetadata(new SendsMetadata(typeof(TMessage)));
    }
}
