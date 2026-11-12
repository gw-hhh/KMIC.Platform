using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>
/// 字典类型的仓储
/// </summary>
public interface IDictionaryTypeRepository : IRepository<DictionaryType, DictionaryTypeId>
{
    /// <summary>
    /// 按编码取要修改的字典类型（连同全部字典项），传了期望版本号时版本不一致返回版本冲突
    /// </summary>
    Task<Result<DictionaryType>> GetByCodeForUpdateAsync(string typeCode, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 全部字典类型，不带字典项：启动时同步代码里的声明用
    /// </summary>
    Task<IReadOnlyList<DictionaryType>> ListWithoutItemsAsync(CancellationToken ct);

    /// <summary>
    /// 同步声明之前先拿到这把锁，直到事务结束：多个实例同时启动时，只有一个真正写入，后到的看到的是已经同步好的结果
    /// </summary>
    Task LockCatalogSyncAsync(CancellationToken ct);
}
