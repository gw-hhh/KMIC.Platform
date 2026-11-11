namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 用例以外对权限点的引用
/// </summary>
/// <param name="Permission">权限点编码</param>
/// <param name="Source">引用方，如“资源动作 SampleActions”</param>
public sealed record PermissionReference(string Permission, string Source);
