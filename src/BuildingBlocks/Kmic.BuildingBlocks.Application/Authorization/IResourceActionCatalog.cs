namespace Kmic.BuildingBlocks.Application.Authorization;

// 服务注册阶段尚未确定资源与动作的泛型参数，通过此非泛型接口读取权限点
internal interface IResourceActionCatalog
{
    IEnumerable<string> Permissions { get; }
}
