using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Domain.Settings;

public interface ISystemSettingRepository : IRepository<SystemSettingValue, SystemSettingValueId>
{
    /// <summary>按设置键读取待修改的值（未指定版本时要求当前尚未保存值）</summary>
    Task<Result<SystemSettingValue?>> GetByKeyForUpdateAsync(string key, long? expectedVersion, CancellationToken ct);

    /// <summary>登记变更记录，随当前事务一并保存</summary>
    void AddChange(SystemSettingChange change);
}
