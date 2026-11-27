namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>用例以外（如资源动作、系统设置等）对权限点的引用。</summary>
public sealed record PermissionReference(string Permission, string Source);
