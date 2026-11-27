namespace Kmic.BuildingBlocks.Application.Jobs;

public interface IModuleJobStores
{
    /// <summary>已注册数据库的模块作业存储列表，按模块名排序。</summary>
    IReadOnlyList<IModuleJobStore> All { get; }

    /// <summary>按模块名获取作业存储。</summary>
    IModuleJobStore? Find(string module);
}
