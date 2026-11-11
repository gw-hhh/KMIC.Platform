namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 标记端点属于哪个模块，由模块路由组写入
/// </summary>
/// <param name="Module">模块名</param>
public sealed record ModuleMetadata(string Module);
