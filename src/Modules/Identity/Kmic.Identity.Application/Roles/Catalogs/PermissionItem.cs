namespace Kmic.Identity.Application.Roles.Catalogs;

/// <summary>权限点目录项。</summary>
/// <param name="Code">编码</param>
/// <param name="Module">所属模块</param>
/// <param name="Group">分组</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Sensitive">是否敏感</param>
public sealed record PermissionItem(string Code, string Module, string Group, string DisplayName, bool Sensitive);
