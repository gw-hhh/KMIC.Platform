namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>直连数据库读取的系统设置值及其保存时间；未保存过时 <paramref name="Value"/> 为代码默认值、<paramref name="SavedAt"/> 为 null。</summary>
public sealed record FreshSetting<T>(T Value, DateTimeOffset? SavedAt)
    where T : class, ISystemSetting<T>;
