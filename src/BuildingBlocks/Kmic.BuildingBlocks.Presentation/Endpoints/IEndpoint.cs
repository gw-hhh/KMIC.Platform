namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 一个接口一个端点类
/// </summary>
/// <remarks>
/// 端点只做三件事：绑定请求、构造消息并发送、把 Result 转成 HTTP。写成 internal sealed 类，类名以 Endpoint 结尾，
/// 启动时由所在模块的 <see cref="ModuleEndpoints"/> 扫描并调用 <see cref="Map"/>
/// </remarks>
public interface IEndpoint
{
    /// <summary>
    /// 把本端点映射到路由组上
    /// </summary>
    void Map(IEndpointRouteBuilder group);
}
