namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 不经缓存读取系统设置，并带上保存时间
/// </summary>
/// <remarks>
/// 给基础设施里的后台判断用：需要知道“这个值多久没更新”，或不能容忍缓存延迟。每次都读库，业务用例读设置走 <see cref="ISettings" />，不注入它
/// </remarks>
public interface IFreshSettings
{
    /// <summary>
    /// 读取最新保存的设置值和保存时间，没保存过时是默认值、保存时间为空
    /// </summary>
    ValueTask<FreshSetting<T>> GetAsync<T>(CancellationToken ct)
        where T : class, ISystemSetting<T>;
}
