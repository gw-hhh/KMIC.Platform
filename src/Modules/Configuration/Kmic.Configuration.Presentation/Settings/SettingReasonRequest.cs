namespace Kmic.Configuration.Presentation.Settings;

/// <summary>
/// 恢复默认、清除设置值的请求
/// </summary>
/// <param name="Reason">原因</param>
/// <param name="ExpectedVersion">预期版本号</param>
public sealed record SettingReasonRequest(string Reason, long ExpectedVersion);
