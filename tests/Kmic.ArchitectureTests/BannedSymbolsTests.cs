using Kmic.ArchitectureTests.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace Kmic.ArchitectureTests;

/// <summary>
/// 禁用 API 清单本身的检查：清单里每一条都要能解析成唯一的符号，并且清单已经接入构建。
/// 方法 ID 写错一个字符、少写一个重载，分析器不报错，只是那一条悄悄不生效，所以在这里逐条核对
/// </summary>
public sealed class BannedSymbolsTests
{
    private static readonly Lazy<CSharpCompilation> _compilation = new(CreateCompilation);

    public static TheoryData<string> Lists => [.. ListFiles().Select(path => Path.GetFileName(path))];

    [Theory]
    [MemberData(nameof(Lists))]
    public void EveryEntry_ResolvesToExactlyOneSymbol(string fileName)
    {
        var problems = new List<string>();
        foreach (var entry in Entries(Path.Combine(ProductAssemblies.RepositoryRoot(), fileName)))
        {
            var matches = DocumentationCommentId.GetSymbolsForDeclarationId(entry.Id, _compilation.Value);
            if (matches.Length != 1)
            {
                problems.Add($"{entry.Id}：解析出 {matches.Length} 个符号（方法要逐个重载写全）");
            }

            if (string.IsNullOrWhiteSpace(entry.Message))
            {
                problems.Add($"{entry.Id}：没有写替代做法");
            }
        }

        problems.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Lists))]
    public void EveryList_IsWiredIntoTheBuild(string fileName) =>
        File.ReadAllText(Path.Combine(ProductAssemblies.RepositoryRoot(), "Directory.Build.props")).ShouldContain(fileName);

    [Fact]
    public void ModuleList_BansTheAgreedSymbols()
    {
        var ids = Entries(Path.Combine(ProductAssemblies.RepositoryRoot(), "BannedSymbols.Modules.txt")).Select(e => e.Id).ToList();

        HasId(ids, "T:Microsoft.Extensions.Configuration.IConfiguration");
        HasId(ids, "T:Microsoft.Extensions.Caching.Memory.IMemoryCache");
        HasId(ids, "T:Microsoft.Extensions.Caching.Distributed.IDistributedCache");
        HasId(ids, "M:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IgnoreQueryFilters``1(System.Linq.IQueryable{``0})");
        HasId(ids, "M:Microsoft.AspNetCore.Http.Results.StatusCode(System.Int32)");
        HasId(ids, "M:System.Threading.Tasks.Task.Run(System.Action)");
    }

    private static void HasId(List<string> ids, string id) => ids.Contains(id, StringComparer.Ordinal).ShouldBeTrue($"清单里没有 {id}");

    private static IEnumerable<string> ListFiles() =>
        Directory.EnumerateFiles(ProductAssemblies.RepositoryRoot(), "BannedSymbols*.txt", SearchOption.TopDirectoryOnly).Order(StringComparer.Ordinal);

    private static IEnumerable<(string Id, string Message)> Entries(string path) =>
        File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .Select(line =>
            {
                var separator = line.IndexOf(';', StringComparison.Ordinal);
                return separator < 0 ? (line, "") : (line[..separator], line[(separator + 1)..]);
            });

    // 当前进程能加载的全部程序集：框架、EF、ASP.NET Core 都在里面，清单里的符号都能找到
    private static CSharpCompilation CreateCompilation() => CSharpCompilation.Create(
        "BannedSymbolsProbe",
        references: ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path)));
}
