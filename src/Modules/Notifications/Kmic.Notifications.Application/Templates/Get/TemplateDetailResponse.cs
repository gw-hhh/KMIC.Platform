using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kmic.Notifications.Application.Templates.Get;

/// <summary>
/// 一个通知模板的详情
/// </summary>
/// <param name="Key">模板键</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Module">声明它的模块</param>
/// <param name="SenderProfile">发件账号名，只对邮件渠道有意义</param>
/// <param name="CarriesSecret">内容含一次性短时凭据</param>
/// <param name="Status">状态编码：Active、Retired</param>
/// <param name="ConcurrencyVersion">并发版本号，发布和恢复时作为期望版本传回，输出为 version</param>
/// <param name="Channels">各渠道当前的内容</param>
/// <param name="Variables">模板里能用的变量名</param>
/// <param name="Example">示例数据，预览默认用它；代码里已经删掉的模板没有</param>
/// <param name="Versions">发布过的版本，新的在前</param>
public sealed record TemplateDetailResponse(
    string Key,
    string DisplayName,
    string Module,
    string SenderProfile,
    bool CarriesSecret,
    string Status,
    [property: JsonPropertyName("version")] long ConcurrencyVersion,
    IReadOnlyList<TemplateChannelInfo> Channels,
    IReadOnlyList<string> Variables,
    JsonElement? Example,
    IReadOnlyList<TemplateVersionInfo> Versions);
