namespace Kmic.BuildingBlocks.Application;

internal static class ModuleNames
{
    // 模块的声明都在 Kmic.{模块}.Application 中
    public static string Of(Type type) =>
        type.Assembly.GetName().Name?.Split('.') is ["Kmic", var module, ..] ? module : type.Assembly.GetName().Name ?? "Unknown";
}
