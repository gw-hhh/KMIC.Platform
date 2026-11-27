namespace Kmic.Notifications.Application.Templates.Preview;

/// <summary>模板预览结果</summary>
/// <param name="Subject">渲染后的主题，渲染失败时为空</param>
/// <param name="Body">渲染后的正文，渲染失败时为空</param>
/// <param name="Error">渲染失败原因，成功时为空</param>
public sealed record TemplatePreviewResponse(string? Subject, string? Body, string? Error);
