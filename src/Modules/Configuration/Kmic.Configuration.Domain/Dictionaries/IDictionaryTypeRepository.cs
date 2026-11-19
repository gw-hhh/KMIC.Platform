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
    /// 同步前取事务级咨询锁，让多个实例的同步依次执行，后到的读到前一个提交的结果
    /// </summary>
    Task LockCatalogSyncAsync(CancellationToken ct);
}
