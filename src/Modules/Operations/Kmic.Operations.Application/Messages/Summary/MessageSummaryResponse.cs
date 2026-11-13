namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 消息积压汇总
/// </summary>
/// <param name="Modules">每个模块一项，按模块名排好</param>
public sealed record MessageSummaryResponse(IReadOnlyList<ModuleBacklogItem> Modules);
