using Kmic.BuildingBlocks.Analyzers.General;
using Ascii = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.General.AsciiIdentifierAnalyzer>;
using Conditional = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.General.NestedConditionalAnalyzer>;
using Dynamic = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.General.DynamicAnalyzer>;
using Justification = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.General.SuppressionJustificationAnalyzer>;
using Marker = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.General.TrackedMarkerAnalyzer>;
using Pragma = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.General.PragmaWarningAnalyzer>;
using PrimaryParameter = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.General.PrimaryConstructorParameterAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.General;

public sealed class GeneralRuleTests
{
    private const string Project = "Kmic.Sample.Application";

    [Fact]
    public async Task SuppressMessage_RequiresJustification()
    {
        const string Source = """
            using System.Diagnostics.CodeAnalysis;

            public static class Sample
            {
                [SuppressMessage("Design", "CA1031", Justification = "后台循环必须吞掉单条失败，失败已记入死信")]
                public static void WithReason() { }

                [{|#0:SuppressMessage("Design", "CA1031")|}]
                public static void WithoutReason() { }

                [{|#1:SuppressMessage("Design", "CA1031", Justification = " ")|}]
                public static void BlankReason() { }
            }
            """;

        await Justification.VerifyAsync(
            Source,
            Project,
            Justification.Diagnostic(SuppressionJustificationAnalyzer.Rule, 0, "SuppressMessage"),
            Justification.Diagnostic(SuppressionJustificationAnalyzer.Rule, 1, "SuppressMessage"));
    }

    [Fact]
    public async Task PragmaWarningDisable_Reports_RestoreAllowed()
    {
        const string Source = """
            public static class Sample
            {
            {|#0:#pragma warning disable CS0168|}
                public static void Run() { int unused; }
            #pragma warning restore CS0168
            }
            """;

        await Pragma.VerifyAsync(Source, Project, Pragma.Diagnostic(PragmaWarningAnalyzer.Rule, 0));
    }

    [Fact]
    public async Task TodoFixmeHackAndSkip_RequireTicket()
    {
        const string Source = """
            using System;

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class FactAttribute : Attribute
            {
                public string? Skip { get; set; }
            }

            public static class SampleTests
            {
                // TODO(#12): 等接口上线后改成真实调用
                // {|#0:TODO|}: 以后再说
                /* {|#1:FIXME|} 这里有问题 */
                // {|#2:HACK|} 绕一下
                [Fact(Skip = "#34 等待接口上线")]
                public static void Tracked() { }

                [Fact({|#3:Skip = "暂时跳过"|})]
                public static void Untracked() { }
            }
            """;

        await Marker.VerifyAsync(
            Source,
            Project,
            Marker.Diagnostic(TrackedMarkerAnalyzer.UntrackedMarker, 0, "TODO"),
            Marker.Diagnostic(TrackedMarkerAnalyzer.UntrackedMarker, 1, "FIXME"),
            Marker.Diagnostic(TrackedMarkerAnalyzer.Hack, 2),
            Marker.Diagnostic(TrackedMarkerAnalyzer.UntrackedSkip, 3));
    }

    [Fact]
    public async Task NonAsciiIdentifier_ReportsDeclarationsOnly()
    {
        const string Source = """
            public sealed class {|#0:订单|}
            {
                public int {|#1:数量|} { get; set; }

                public string Name { get; set; } = "中文字符串可以";

                public int Double() => 数量 * 2;
            }
            """;

        await Ascii.VerifyAsync(
            Source,
            Project,
            Ascii.Diagnostic(AsciiIdentifierAnalyzer.Rule, 0, "订单"),
            Ascii.Diagnostic(AsciiIdentifierAnalyzer.Rule, 1, "数量"));
    }

    [Fact]
    public async Task PrimaryConstructorParameter_Reassigned_Reports()
    {
        const string Source = """
            public sealed class Counter(int start)
            {
                public int Current => start;

                public void Reset() => {|#0:start|} = 0;

                public void Next() => {|#1:start|}++;

                public static int Local(int value)
                {
                    value++;
                    return value;
                }
            }
            """;

        await PrimaryParameter.VerifyAsync(
            Source,
            Project,
            PrimaryParameter.Diagnostic(PrimaryConstructorParameterAnalyzer.Rule, 0, "start"),
            PrimaryParameter.Diagnostic(PrimaryConstructorParameterAnalyzer.Rule, 1, "start"));
    }

    [Fact]
    public async Task Dynamic_Reports()
    {
        const string Source = """
            public static class Sample
            {
                public static object Echo(object value) => value;

                public static {|#0:dynamic|} Load() => 1;
            }
            """;

        await Dynamic.VerifyAsync(Source, Project, Dynamic.Diagnostic(DynamicAnalyzer.Rule, 0));
    }

    [Fact]
    public async Task NestedConditional_Reports_LambdaBoundaryAllowed()
    {
        const string Source = """
            using System;

            public static class Sample
            {
                public static string Simple(bool a) => a ? "是" : "否";

                public static string Nested(bool a, bool b) => a ? "甲" : ({|#0:b ? "乙" : "丙"|});

                public static Func<bool, string> Lambda(bool a) => a ? x => x ? "是" : "否" : _ => "无";
            }
            """;

        await Conditional.VerifyAsync(Source, Project, Conditional.Diagnostic(NestedConditionalAnalyzer.Rule, 0));
    }
}
