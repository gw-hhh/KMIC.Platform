using Kmic.ArchitectureTests.Support;

namespace Kmic.ArchitectureTests.Rules;

/// <summary>
/// 依赖方向：框架按层引用，模块某一层只能引用框架的同层或更内层构件，模块之间只能引用对方的 Contracts
/// </summary>
internal static class DependencyRules
{
    private const string Framework = ProductAssemblies.FrameworkModule;

    /// <summary>
    /// 一个程序集引用了哪些不该引用的程序集
    /// </summary>
    /// <param name="module">程序集所属的模块，框架构件是 BuildingBlocks</param>
    /// <param name="layer">层</param>
    /// <param name="references">它引用的程序集名</param>
    public static IReadOnlyList<string> Check(string module, Layer layer, IEnumerable<string> references)
    {
        var allowed = AllowedNames(module, layer);
        var otherContracts = layer is Layer.Application or Layer.Infrastructure;
        return
        [
            .. references
                .Where(r => r.StartsWith("Kmic.", StringComparison.Ordinal) && !allowed.Contains(r) && !(otherContracts && IsOtherModuleContracts(r)))
                .Select(r => $"Kmic.{module}.{layer} 引用了 {r}，{(string.Equals(module, Framework, StringComparison.Ordinal) ? "框架的" : "模块的")} {layer} 层不允许引用它。"),
        ];
    }

    // 其他模块的 Contracts：模块之间唯一允许的依赖
    private static bool IsOtherModuleContracts(string name) =>
        name.EndsWith(".Contracts", StringComparison.Ordinal) && !name.StartsWith($"Kmic.{Framework}.", StringComparison.Ordinal);

    // 每一层允许引用的程序集；除了这里写的，其他 Kmic.* 程序集都不允许
    private static HashSet<string> AllowedNames(string module, Layer layer)
    {
        string Core(Layer l) => $"Kmic.{Framework}.{l}";
        string Own(Layer l) => $"Kmic.{module}.{l}";

        string[] names = layer switch
        {
            Layer.Domain => [Core(Layer.Domain)],

            // Contracts 用 Result、Error，所以编译时能看到框架的 Domain 程序集；领域类型不出现在 Contracts 里由内容规则检查
            Layer.Contracts => [Core(Layer.Contracts), Core(Layer.Domain)],
            Layer.Application => [Core(Layer.Application), Core(Layer.Domain), Core(Layer.Contracts), Own(Layer.Domain), Own(Layer.Contracts)],
            Layer.Infrastructure =>
            [
                Core(Layer.Infrastructure), Core(Layer.Application), Core(Layer.Domain), Core(Layer.Contracts),
                Own(Layer.Application), Own(Layer.Domain), Own(Layer.Contracts),
            ],
            Layer.Presentation =>
            [
                Core(Layer.Presentation), Core(Layer.Application), Core(Layer.Domain), Core(Layer.Contracts),
                Own(Layer.Application), Own(Layer.Domain), Own(Layer.Contracts),
            ],
            _ => [],
        };
        return new HashSet<string>(names, StringComparer.Ordinal);
    }
}
