namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 不经缓存读到的设置值和它的保存时间
/// </summary>
/// <typeparam name="T">设置的值类型</typeparam>
/// <param name="Value">值，没保存过时是代码默认值</param>
/// <param name="SavedAt">最后一次保存的时间；没保存过时为空</param>
public sealed record FreshSetting<T>(T Value, DateTimeOffset? SavedAt)
    where T : class, ISystemSetting<T>;
