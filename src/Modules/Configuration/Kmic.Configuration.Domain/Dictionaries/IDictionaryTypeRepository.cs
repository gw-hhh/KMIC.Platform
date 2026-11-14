using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>
/// 字典类型的仓储
/// </summary>
public interface IDictionaryTypeRepository : IRepository<DictionaryType, DictionaryTypeId>
{
    /// <summary>
    /// 按编码加载待修改的字典类型及全部字典项，指定预期版本号时校验版本
    /// </summary>
    Task<Result<DictionaryType>> GetByCodeForUpdateAsync(string typeCode, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 全部字典类型，不带字典项：启动时同步代码里的声明用
    /// </summary>
    Task<IReadOnlyList<DictionaryType>> ListWithoutItemsAsync(CancellationToken ct);

    /// <summary>
    /// 同步前取得事务级咨询锁并持有至事务结束，使多实例同步串行执行，后续实例读取已提交的同步结果
    /// </summary>
    Task LockCatalogSyncAsync(CancellationToken ct);
}
