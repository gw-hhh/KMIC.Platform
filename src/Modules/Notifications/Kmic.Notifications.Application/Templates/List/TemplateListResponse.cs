namespace Kmic.Notifications.Application.Templates.List;

/// <summary>
/// 模板列表，按模板键排序
/// </summary>
/// <param name="Items">模板</param>
public sealed record TemplateListResponse(IReadOnlyList<TemplateListItem> Items);
