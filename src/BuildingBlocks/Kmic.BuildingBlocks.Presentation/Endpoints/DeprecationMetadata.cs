namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 端点已废弃，由 <c>Deprecated(...)</c> 写入端点元数据
/// </summary>
/// <param name="Since">从哪天起废弃</param>
/// <param name="Sunset">从哪天起不再提供</param>
public sealed record DeprecationMetadata(DateOnly Since, DateOnly Sunset);
