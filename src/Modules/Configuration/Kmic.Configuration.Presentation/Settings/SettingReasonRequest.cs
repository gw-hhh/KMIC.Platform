namespace Kmic.Configuration.Presentation.Settings;

/// <summary>
/// 恢复默认、清除设置值的请求
/// </summary>
/// <param name="Reason">原因，必填，最多 500 个字符，记入变更历史</param>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回 409 Common.VersionConflict</param>
public sealed record SettingReasonRequest(string Reason, long ExpectedVersion);
