namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 权限点目录中的一项
/// </summary>
/// <param name="Code">编码，格式“模块.资源.动作”</param>
/// <param name="Module">所属模块</param>
/// <param name="Group">分组显示名</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Sensitive">是否敏感</param>
public sealed record PermissionDefinition(string Code, string Module, string Group, string DisplayName, bool Sensitive);
