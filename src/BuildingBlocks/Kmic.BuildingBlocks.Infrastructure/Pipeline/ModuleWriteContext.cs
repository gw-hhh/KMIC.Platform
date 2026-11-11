namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 模块的写入上下文
/// </summary>
internal sealed record ModuleWriteContext(string Module, Type ContextType);
