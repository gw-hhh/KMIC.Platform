using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Domain.Settings;

/// <summary>
/// 系统设置值的仓储
/// </summary>
public interface ISystemSettingRepository : IRepository<SystemSettingValue, SystemSettingValueId>
{
    /// <summary>
    /// 按设置键取要修改的值行：没有这一行（设置用的是默认值）时得到空
    /// </summary>
    /// <param name="key">设置键</param>
    /// <param name="expectedVersion">调用方看到的版本号；空表示调用方看到的是默认值，这时已经有这一行就是版本冲突</param>
    /// <param name="ct">取消令牌</param>
    Task<Result<SystemSettingValue?>> GetByKeyForUpdateAsync(string key, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 登记一次变更，随命令的事务一起保存
    /// </summary>
    void AddChange(SystemSettingChange change);
}
