namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 一个接口一个端点类
/// </summary>
/// <remarks>
/// 端点只绑定请求、发送消息、把 Result 转成 HTTP
/// </remarks>
public interface IEndpoint
{
    /// <summary>
    /// 把本端点映射到路由组上
    /// </summary>
    void Map(IEndpointRouteBuilder group);
}
