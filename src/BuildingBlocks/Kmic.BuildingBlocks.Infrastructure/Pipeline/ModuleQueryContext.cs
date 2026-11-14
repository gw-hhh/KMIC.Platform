namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 模块的查询上下文
/// </summary>
internal sealed record ModuleQueryContext(string Module, Type ContextType);
