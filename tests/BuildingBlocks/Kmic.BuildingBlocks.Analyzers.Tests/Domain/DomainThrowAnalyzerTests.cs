using Kmic.BuildingBlocks.Analyzers.Domain;
using Verify = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.Domain.DomainThrowAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Domain;

public sealed class DomainThrowAnalyzerTests
{
    private const string Source = """
        using System;
        using Kmic.BuildingBlocks.Domain.Entities;

        public sealed class Sample
        {
            public void Allowed(string? title, int count)
            {
                ArgumentNullException.ThrowIfNull(title);
                if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count)); }
                if (count > 99) { throw new InvalidOperationException("不可能"); }
                _ = title.Length > 0 ? title : throw new DomainInvariantException("标题为空");
                try { Allowed(title, count); } catch (FormatException) { throw; }
            }

            public void Forbidden(bool flag)
            {
                if (flag) { {|#0:throw new NotSupportedException();|} }
                _ = flag ? 1 : {|#1:throw new ApplicationException("业务失败")|};
            }
        }
        """;

    [Fact]
    public async Task Throw_InModuleDomain_OnlyAllowedTypesPass()
    {
        await Verify.VerifyAsync(
            Source,
            Verify.ModuleDomain,
            Verify.Diagnostic(DomainThrowAnalyzer.Rule, 0, "NotSupportedException"),
            Verify.Diagnostic(DomainThrowAnalyzer.Rule, 1, "ApplicationException"));
    }

    [Fact]
    public async Task Throw_OutsideModuleDomain_NoDiagnostic()
    {
        await Verify.VerifyAsync(Source.Replace("{|#0:", string.Empty, StringComparison.Ordinal).Replace("{|#1:", string.Empty, StringComparison.Ordinal).Replace("|}", string.Empty, StringComparison.Ordinal), "Kmic.Sample.Application");
    }
}
