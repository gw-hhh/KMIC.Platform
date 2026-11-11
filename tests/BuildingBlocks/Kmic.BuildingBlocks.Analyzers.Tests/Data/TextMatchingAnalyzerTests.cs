using Kmic.BuildingBlocks.Analyzers.Data;
using Verifier = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.Data.TextMatchingAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Data;

/// <summary>
/// 只拦截数据库查询里的字符串匹配，内存里的 LINQ 照常可用
/// </summary>
public sealed class TextMatchingAnalyzerTests
{
    private const string Source = """
        using System.Collections.Generic;
        using System.Linq;

        public sealed record Item(string Name, string Code);

        public static class Queries
        {
            public static object Lambdas(IQueryable<Item> items, string q) => items.Where(i =>
                i.Name.{|#0:Contains|}(q) || i.Code.{|#1:StartsWith|}(q) || i.Name.{|#2:EndsWith|}(q) || i.Code.{|#3:ToLower|}() == q);

            public static object QuerySyntax(IQueryable<Item> items, string q) =>
                from i in items where i.Name.{|#4:ToUpperInvariant|}() == q select i;

            public static object InMemory(IEnumerable<Item> items, string q) =>
                items.Where(i => i.Name.Contains(q) || i.Code.ToLower() == q).ToList();

            public static object CollectionContains(IQueryable<Item> items, List<string> codes) =>
                items.Where(i => codes.Contains(i.Code));

            public static bool OutsideQuery(string name, string q) => name.Contains(q);
        }
        """;

    [Fact]
    public async Task ModuleApplication_ReportsStringMatchingInQueriesOnly()
    {
        await Verifier.VerifyAsync(
            Source,
            "Kmic.Sample.Application",
            Verifier.Diagnostic(TextMatchingAnalyzer.Rule, 0, "Contains"),
            Verifier.Diagnostic(TextMatchingAnalyzer.Rule, 1, "StartsWith"),
            Verifier.Diagnostic(TextMatchingAnalyzer.Rule, 2, "EndsWith"),
            Verifier.Diagnostic(TextMatchingAnalyzer.Rule, 3, "ToLower"),
            Verifier.Diagnostic(TextMatchingAnalyzer.Rule, 4, "ToUpperInvariant"));
    }

    [Theory]
    [InlineData("Kmic.Sample.Domain")]
    [InlineData("Kmic.BuildingBlocks.Infrastructure")]
    [InlineData("Kmic.Sample.Application.Tests")]
    public async Task OtherProjects_NotChecked(string assemblyName)
    {
        await Verifier.VerifyAsync(Source.Replace("{|#0:", "", StringComparison.Ordinal)
            .Replace("{|#1:", "", StringComparison.Ordinal)
            .Replace("{|#2:", "", StringComparison.Ordinal)
            .Replace("{|#3:", "", StringComparison.Ordinal)
            .Replace("{|#4:", "", StringComparison.Ordinal)
            .Replace("|}", "", StringComparison.Ordinal), assemblyName);
    }
}
