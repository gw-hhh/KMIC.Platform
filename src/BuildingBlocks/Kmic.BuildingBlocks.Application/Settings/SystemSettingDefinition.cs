using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

public sealed class SystemSettingDefinition : DeclaredValueDefinition
{
    internal SystemSettingDefinition(SystemSettingAttribute declaration, Type valueType, object defaultValue, Func<object, Result> validate)
        : base(declaration.Key, declaration.DisplayName, valueType, defaultValue, validate)
    {
        Group = declaration.Group;
        EditPermission = declaration.EditPermission;
    }

    public string? Group { get; }

    public string EditPermission { get; }
}
