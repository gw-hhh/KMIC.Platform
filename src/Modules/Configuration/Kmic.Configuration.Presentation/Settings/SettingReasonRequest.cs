namespace Kmic.Configuration.Presentation.Settings;

/// <param name="Reason">原因</param>
/// <param name="ExpectedVersion">预期版本号</param>
public sealed record SettingReasonRequest(string Reason, long ExpectedVersion);
