namespace Kmic.Notifications.Application.Templates.Preview;

/// <summary>
/// 预览的结果
/// </summary>
/// <param name="Subject">渲染后的主题；模板有误时为空</param>
/// <param name="Body">渲染后的正文；模板有误时为空</param>
/// <param name="Error">模板错误原因，无错误时为空</param>
public sealed record TemplatePreviewResponse(string? Subject, string? Body, string? Error);
