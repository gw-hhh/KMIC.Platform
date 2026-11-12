namespace Kmic.Identity.Application.Roles.Catalogs;

/// <summary>
/// 数据范围对象目录里的一项
/// </summary>
/// <param name="Code">编码</param>
/// <param name="Module">所属模块</param>
/// <param name="DisplayName">显示名</param>
public sealed record DataScopeObjectItem(string Code, string Module, string DisplayName);
