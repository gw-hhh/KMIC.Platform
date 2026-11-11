using Kmic.BuildingBlocks.Analyzers.Generators;
using Kmic.BuildingBlocks.Domain.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Generators;

/// <summary>
/// 实体与值对象公开属性的 summary 收集为注释表；其他类型、没有注释或不公开的属性不收集
/// </summary>
public sealed class EntityCommentGeneratorTests
{
    private const string Source = """
        using Kmic.BuildingBlocks.Domain.Entities;

        namespace Kmic.Sample.Domain;

        public sealed class Sample : Entity<System.Guid>
        {
            /// <summary>
            /// 标题；保存前去除首尾空格
            /// </summary>
            public string Title { get; private set; } = "";

            /// <summary>
            /// 截止日期，含义同 <see cref="P:Kmic.Sample.Domain.Sample.Title"/>
            /// </summary>
            public System.DateOnly? DueDate { get; private set; }

            public int Undocumented { get; private set; }

            /// <summary>
            /// 内部状态
            /// </summary>
            internal int Hidden { get; private set; }
        }

        public sealed record Money
        {
            /// <summary>
            /// 金额
            /// </summary>
            public decimal Amount { get; private init; }
        }

        public sealed class Helper
        {
            /// <summary>
            /// 不是实体
            /// </summary>
            public int Value { get; set; }
        }
        """;

    [Fact]
    public void Generate_CollectsEntityAndValueObjectSummaries()
    {
        var generated = Run(Source);

        Assert.Contains("""["Kmic.Sample.Domain.Sample.Title"] = "标题；保存前去除首尾空格",""", generated, StringComparison.Ordinal);
        Assert.Contains("""["Kmic.Sample.Domain.Sample.DueDate"] = "截止日期，含义同 Title",""", generated, StringComparison.Ordinal);
        Assert.Contains("""["Kmic.Sample.Domain.Money.Amount"] = "金额",""", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("Undocumented", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("Hidden", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("Helper", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_WithoutEntitiesOrValueObjects_EmitsNothing()
    {
        const string Plain = """
            public sealed class Plain
            {
                /// <summary>
                /// 说明
                /// </summary>
                public int Value { get; set; }
            }
            """;

        Assert.Empty(Run(Plain));
    }

    private static string Run(string source)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse);
        var compilation = CSharpCompilation.Create(
            "Kmic.Sample.Domain",
            [CSharpSyntaxTree.ParseText(source, parseOptions, cancellationToken: TestContext.Current.CancellationToken)],
            [.. PlatformReferences(), MetadataReference.CreateFromFile(typeof(Entity<>).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver.Create([new EntityCommentGenerator().AsSourceGenerator()], parseOptions: parseOptions);

        var result = driver.RunGenerators(compilation, TestContext.Current.CancellationToken).GetRunResult();
        return string.Concat(result.GeneratedTrees.Select(t => t.ToString()));
    }

    // 运行测试的框架程序集足以编译这些源码
    private static IEnumerable<MetadataReference> PlatformReferences() =>
        (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path));
}
