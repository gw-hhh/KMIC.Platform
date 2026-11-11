namespace Kmic.BuildingBlocks.Application.Authorization;

// 注册时不知道资源和动作的类型参数，通过它读取权限点
internal interface IResourceActionCatalog
{
    IEnumerable<string> Permissions { get; }
}
