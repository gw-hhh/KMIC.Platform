namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 全部模块的后台作业存储
/// </summary>
public interface IModuleJobStores
{
    /// <summary>
    /// 每个注册了数据库的模块一个，按模块名排好
    /// </summary>
    IReadOnlyList<IModuleJobStore> All { get; }

    /// <summary>
    /// 按模块名取，没有这个模块时返回空
    /// </summary>
    /// <param name="module">模块名</param>
    IModuleJobStore? Find(string module);
}
