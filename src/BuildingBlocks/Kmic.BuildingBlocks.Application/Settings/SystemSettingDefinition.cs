using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 系统设置定义
/// </summary>
public sealed class SystemSettingDefinition : DeclaredValueDefinition
{
    internal SystemSettingDefinition(SystemSettingAttribute declaration, Type valueType, object defaultValue, Func<object, Result> validate)
        : base(declaration.Key, declaration.DisplayName, valueType, defaultValue, validate)
    {
        Group = declaration.Group;
        EditPermission = declaration.EditPermission;
    }

    /// <summary>
    /// 分组
    /// </summary>
    public string? Group { get; }

    /// <summary>
    /// 修改所需的权限点
    /// </summary>
    public string EditPermission { get; }
}
