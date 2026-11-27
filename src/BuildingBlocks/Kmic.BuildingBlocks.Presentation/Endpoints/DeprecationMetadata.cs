namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>端点废弃元数据（废弃生效日与计划下线日）</summary>
public sealed record DeprecationMetadata(DateOnly Since, DateOnly Sunset);
