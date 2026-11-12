using System.Reflection;

namespace Kmic.ArchitectureTests.Support;

/// <summary>
/// 全部产品程序集：框架与各业务模块，测试项目、测试基础设施和宿主不在其中
/// </summary>
internal static class ProductAssemblies
{
    /// <summary>
    /// 框架构件的“模块名”
    /// </summary>
    public const string FrameworkModule = "BuildingBlocks";

    // 测试基础设施和宿主组装库可以引用任何层，不受层次规则约束
    private static readonly string[] _exempt = ["Testing", "Hosting", "Analyzers"];

    public static IReadOnlyList<ProductAssembly> All { get; } = Discover();

    public static IReadOnlyList<ProductAssembly> Modules => [.. All.Where(a => a.IsModule)];

    public static IReadOnlyList<ProductAssembly> Framework => [.. All.Where(a => !a.IsModule)];

    public static IEnumerable<string> ModuleNames => Modules.Select(a => a.Module).Distinct(StringComparer.Ordinal);

    public static IEnumerable<ProductAssembly> Of(Layer layer, bool modulesOnly = true) => All.Where(a => a.Layer == layer && (!modulesOnly || a.IsModule));

    /// <summary>
    /// 仓库根目录：向上找解决方案文件
    /// </summary>
    public static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Kmic.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("找不到仓库根目录（Kmic.slnx）。");
    }

    private static List<ProductAssembly> Discover()
    {
        var found = new List<ProductAssembly>();
        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "Kmic.*.dll"))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var parts = name.Split('.');
            if (parts.Length != 3 || !Enum.TryParse<Layer>(parts[2], out var layer) || _exempt.Contains(parts[2], StringComparer.Ordinal))
            {
                continue;
            }

            found.Add(new ProductAssembly(Assembly.Load(AssemblyName.GetAssemblyName(path)), parts[1], layer));
        }

        return found;
    }
}
