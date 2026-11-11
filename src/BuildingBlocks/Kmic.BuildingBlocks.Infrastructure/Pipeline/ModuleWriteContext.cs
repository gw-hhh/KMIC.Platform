namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 一个模块的写入上下文类型；模块名按程序集名 Kmic.{模块}.* 推导，与用例目录一致
/// </summary>
internal sealed record ModuleWriteContext(string Module, Type ContextType);
