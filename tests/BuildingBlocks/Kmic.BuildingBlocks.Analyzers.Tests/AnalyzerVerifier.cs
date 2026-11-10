using Kmic.BuildingBlocks.Analyzers.General;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Kmic.BuildingBlocks.Analyzers.Tests;

/// <summary>
/// 在指定程序集名下编译源码并核对诊断；规则按程序集名判断模块的层
/// </summary>
internal static class AnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public const string ModuleDomain = "Kmic.Sample.Domain";

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor, int location, params object[] arguments) =>
        new DiagnosticResult(descriptor).WithLocation(location).WithArguments(arguments);

    public static Task VerifyAsync(string source, string assemblyName, params DiagnosticResult[] expected) =>
        VerifyAsync([source], assemblyName, expected);

    public static async Task VerifyAsync(string[] sources, string assemblyName, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
        };
        foreach (var source in sources)
        {
            test.TestState.Sources.Add(source);
        }

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(Error).Assembly.Location));
        test.SolutionTransforms.Add((solution, projectId) => solution.WithProjectAssemblyName(projectId, assemblyName));
        // 测试框架会在文件开头插入 #pragma warning disable 来检验分析器是否尊重豁免，而 KM9002 禁止的正是这句
        if (typeof(TAnalyzer) == typeof(PragmaWarningAnalyzer))
        {
            test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
        }

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
