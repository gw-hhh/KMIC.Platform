namespace Kmic.BuildingBlocks.Application;

internal static class ModuleNames
{
    // 模块声明程序集按 Kmic.{模块}.Application 命名
    public static string Of(Type type) =>
        type.Assembly.GetName().Name?.Split('.') is ["Kmic", var module, ..] ? module : type.Assembly.GetName().Name ?? "Unknown";
}
