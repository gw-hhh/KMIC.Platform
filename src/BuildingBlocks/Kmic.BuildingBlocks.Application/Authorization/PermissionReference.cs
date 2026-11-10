namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 用例以外对权限点的引用（资源动作、设置的修改权限等），权限点目录据此核对存在性与“无死权限”
/// </summary>
/// <param name="Permission">权限点编码</param>
/// <param name="Source">引用方，用于错误信息，如“资源动作 SampleActions”</param>
public sealed record PermissionReference(string Permission, string Source);
