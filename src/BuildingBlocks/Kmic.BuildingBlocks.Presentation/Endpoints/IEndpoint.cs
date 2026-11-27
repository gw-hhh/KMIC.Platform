namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>API 端点契约：仅负责请求绑定、发送消息及将 Result 转为 HTTP 响应</summary>
public interface IEndpoint
{
    /// <summary>将当前端点映射到指定路由组</summary>
    void Map(IEndpointRouteBuilder group);
}
