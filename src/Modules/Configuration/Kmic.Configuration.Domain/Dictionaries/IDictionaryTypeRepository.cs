using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Domain.Dictionaries;

public interface IDictionaryTypeRepository : IRepository<DictionaryType, DictionaryTypeId>
{
    /// <summary>按编码加载待修改的字典类型及全部字典项（指定预期版本号时校验版本）</summary>
    Task<Result<DictionaryType>> GetByCodeForUpdateAsync(string typeCode, long? expectedVersion, CancellationToken ct);

    /// <summary>获取全部字典类型（不含字典项），供启动同步使用</summary>
    Task<IReadOnlyList<DictionaryType>> ListWithoutItemsAsync(CancellationToken ct);

    /// <summary>获取事务级咨询锁，使多实例启动同步顺序执行</summary>
    Task LockCatalogSyncAsync(CancellationToken ct);
}
