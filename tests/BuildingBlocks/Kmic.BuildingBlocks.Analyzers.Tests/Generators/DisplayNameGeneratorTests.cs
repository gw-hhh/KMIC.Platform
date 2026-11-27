using Kmic.BuildingBlocks.Analyzers.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Generators;

public sealed class DisplayNameGeneratorTests
{
    private const string Source = """
        namespace Kmic.Sample.Application;

        /// <summary>
        /// 新建示例
        /// </summary>
        /// <param name="LoginName">登录名，唯一，不区分大小写</param>
        /// <param name="ParentCode">父项编码</param>
        /// <param name="Note">这是一整句很长的说明而不是一个名称所以不该当成名称用</param>
        /// <param name="Code">code</param>
        public sealed record CreateThingCommand(string LoginName, string? ParentCode, string Note, string Code, int Undocumented);

        public sealed class ThingLine
        {
            /// <summary>
            /// 数量（按基本单位）
            /// </summary>
            public decimal Quantity { get; init; }

            /// <summary>
            /// 内部用
            /// </summary>
            internal int Hidden { get; init; }
        }
        """;

    [Fact]
    public void Generate_TakesTheFirstPhraseOfParamAndSummary()
    {
        var generated = Run(Source, "Kmic.Sample.Application");

        Assert.Contains("""["Kmic.Sample.Application.CreateThingCommand.LoginName"] = "登录名",""", generated, StringComparison.Ordinal);
        Assert.Contains("""["Kmic.Sample.Application.CreateThingCommand.ParentCode"] = "父项编码",""", generated, StringComparison.Ordinal);
        Assert.Contains("""["Kmic.Sample.Application.ThingLine.Quantity"] = "数量",""", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_SkipsSentencesTextWithoutChineseAndUndocumentedMembers()
    {
        var generated = Run(Source, "Kmic.Sample.Application");

        Assert.DoesNotContain(".Note", generated, StringComparison.Ordinal);
        Assert.DoesNotContain(".Code\"", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("Undocumented", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("Hidden", generated, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Kmic.Sample.Domain")]
    [InlineData("Kmic.Sample.Infrastructure")]
    [InlineData("Kmic.Sample.Application.Tests")]
    public void Generate_OutsideApplicationAndContracts_EmitsNothing(string assemblyName) =>
        Assert.Empty(Run(Source, assemblyName));

    [Theory]
    [InlineData("Kmic.Sample.Contracts")]
    [InlineData("Kmic.BuildingBlocks.Application")]
    public void Generate_InContractsAndTheFrameworkApplication_Emits(string assemblyName) =>
        Assert.Contains("登录名", Run(Source, assemblyName), StringComparison.Ordinal);

    private static string Run(string source, string assemblyName)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse);
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, parseOptions, cancellationToken: TestContext.Current.CancellationToken)],
            PlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver.Create([new DisplayNameGenerator().AsSourceGenerator()], parseOptions: parseOptions);

        var result = driver.RunGenerators(compilation, TestContext.Current.CancellationToken).GetRunResult();
        return string.Concat(result.GeneratedTrees.Select(t => t.ToString()));
    }

    // 测试进程已加载的框架程序集足以编译该段源码
    private static IEnumerable<MetadataReference> PlatformReferences() =>
        (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path));
}
