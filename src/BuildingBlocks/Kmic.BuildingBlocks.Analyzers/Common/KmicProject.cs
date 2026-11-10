namespace Kmic.BuildingBlocks.Analyzers.Common;

/// <summary>
/// 按程序集名判断代码所在的模块与层；框架、测试、脚本不属于任何模块层
/// </summary>
internal static class KmicProject
{
    public const string DomainLayer = "Domain";

    private static readonly string[] _layers = ["Domain", "Application", "Infrastructure", "Presentation", "Contracts"];

    /// <summary>
    /// 模块的某一层，如 Kmic.Sample.Domain 返回 Domain；BuildingBlocks 与测试项目返回 null
    /// </summary>
    public static string? ModuleLayer(string? assemblyName)
    {
        if (assemblyName?.Split('.') is not ["Kmic", var module, var layer]
            || string.Equals(module, "BuildingBlocks", StringComparison.Ordinal)
            || !_layers.Contains(layer, StringComparer.Ordinal))
        {
            return null;
        }

        return layer;
    }

    public static bool IsModuleDomain(string? assemblyName) => string.Equals(ModuleLayer(assemblyName), DomainLayer, StringComparison.Ordinal);

    public static bool IsTestProject(string? assemblyName) =>
        assemblyName?.EndsWith("Tests", StringComparison.Ordinal) == true;
}
