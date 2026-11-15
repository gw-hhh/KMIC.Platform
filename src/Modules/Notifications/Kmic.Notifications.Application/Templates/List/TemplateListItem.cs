namespace Kmic.Notifications.Application.Templates.List;

/// <summary>
/// 通知模板列表项
/// </summary>
/// <param name="Key">模板键</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Module">声明它的模块</param>
/// <param name="Channels">声明的渠道编码</param>
/// <param name="CarriesSecret">内容含一次性短时凭据</param>
/// <param name="Status">状态编码：Active、Retired</param>
/// <param name="ActiveVersions">各渠道选用的发布版本号；活动模板未选用版本时使用代码默认内容</param>
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
