namespace Kmic.BuildingBlocks.Analyzers.Common;

internal static class KmicProject
{
    public const string DomainLayer = "Domain";

    private static readonly string[] _layers = ["Domain", "Application", "Infrastructure", "Presentation", "Contracts"];

    // 从程序集名称提取模块分层（如 Kmic.Sample.Domain -> Domain）
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
