namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>绕过缓存直连数据库读取最新系统设置及保存时间，仅供基础设施后台任务使用。</summary>
public interface IFreshSettings
{
    ValueTask<FreshSetting<T>> GetAsync<T>(CancellationToken ct)
        where T : class, ISystemSetting<T>;
}
