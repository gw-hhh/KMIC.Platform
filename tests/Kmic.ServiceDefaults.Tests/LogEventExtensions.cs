using System.Globalization;
using Serilog.Events;

namespace Kmic.ServiceDefaults.Tests;

/// <summary>
/// 读取日志事件属性的便捷方法
/// </summary>
internal static class LogEventExtensions
{
    public static string? Text(this LogEvent logEvent, string name) =>
        logEvent.Properties.TryGetValue(name, out var value) ? Text(value) : null;

    public static string Text(LogEventPropertyValue value) =>
        value is ScalarValue scalar ? Convert.ToString(scalar.Value, CultureInfo.InvariantCulture) ?? "" : value.ToString(null, CultureInfo.InvariantCulture);

    public static bool HasTemplate(this LogEvent logEvent, string template) =>
        string.Equals(logEvent.MessageTemplate.Text, template, StringComparison.Ordinal);

    // 请求摘要日志带路由模板，其他日志没有
    public static bool IsRequestSummary(this LogEvent logEvent) => logEvent.Properties.ContainsKey("RoutePattern");

    public static LogEventPropertyValue Member(this StructureValue structure, string name) =>
        structure.Properties.Single(p => string.Equals(p.Name, name, StringComparison.Ordinal)).Value;
}
