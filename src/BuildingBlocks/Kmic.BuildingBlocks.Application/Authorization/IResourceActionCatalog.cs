namespace Kmic.BuildingBlocks.Application.Authorization;

// 注册时不知道资源与动作的类型参数，经它读取动作目录的权限点
internal interface IResourceActionCatalog
{
    IEnumerable<string> Permissions { get; }
}
