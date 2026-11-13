namespace Kmic.Notifications.Application.Templates.List;

/// <summary>
/// 列表里的一个模板
/// </summary>
/// <param name="Key">模板键</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Module">声明它的模块</param>
/// <param name="Channels">发往的渠道编码</param>
/// <param name="CarriesSecret">内容含一次性短时凭据</param>
/// <param name="Status">状态编码：Active、Retired</param>
/// <param name="ActiveVersions">各渠道生效的版本号，没有的渠道用代码里的默认内容</param>
/// <param name="LastPublishedAt">最近一次发布的时间，没发布过的为空</param>
public sealed record TemplateListItem(
    string Key,
    string DisplayName,
    string Module,
    IReadOnlyList<string> Channels,
    bool CarriesSecret,
    string Status,
    IReadOnlyDictionary<string, int> ActiveVersions,
    DateTimeOffset? LastPublishedAt);
