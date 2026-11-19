using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Domain.Settings;

/// <summary>
/// 系统设置值的仓储
/// </summary>
public interface ISystemSettingRepository : IRepository<SystemSettingValue, SystemSettingValueId>
{
    /// <summary>
    /// 按设置键读取待修改的值
    /// </summary>
    /// <remarks>
    /// 不指定版本时要求还没有保存值
    /// </remarks>
    Task<Result<SystemSettingValue?>> GetByKeyForUpdateAsync(string key, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 登记一次变更，随命令的事务一起保存
    /// </summary>
    void AddChange(SystemSettingChange change);
}
