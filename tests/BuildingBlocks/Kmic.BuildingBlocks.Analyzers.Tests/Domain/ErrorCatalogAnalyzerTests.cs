using Kmic.BuildingBlocks.Analyzers.Domain;
using Verify = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.Domain.ErrorCatalogAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Domain;

public sealed class ErrorCatalogAnalyzerTests
{
    [Fact]
    public async Task Factory_InErrorsClass_NoDiagnostic()
    {
        const string Source = """
            using Kmic.BuildingBlocks.Domain.Results;

            public static class SampleErrors
            {
                public static readonly Error NoItems = Error.BusinessRule("Sample.NoItems", "没有明细");

                public static Error Taken(string name) => Error.Conflict("Sample.Taken", "已被使用").WithDetail(name);
            }
            """;

        await Verify.VerifyAsync(Source, Verify.ModuleDomain);
    }

    [Fact]
    public async Task Factory_OutsideErrorsClass_Reports()
    {
        const string Source = """
            using Kmic.BuildingBlocks.Domain.Results;

            public sealed class Sample
            {
                public Result Submit() => {|#0:Error.BusinessRule("Sample.NoItems", "没有明细")|};
            }

            public sealed class SampleErrors
            {
                public static readonly Error NotStatic = {|#1:Error.NotFound("Sample.NotFound", "不存在")|};
            }
            """;

        await Verify.VerifyAsync(
            Source,
            Verify.ModuleDomain,
            Verify.Diagnostic(ErrorCatalogAnalyzer.OutsideCatalog, 0, "BusinessRule"),
            Verify.Diagnostic(ErrorCatalogAnalyzer.OutsideCatalog, 1, "NotFound"));
    }

    [Fact]
    public async Task Unavailable_InModuleCatalog_Reports()
    {
        const string Source = """
            using Kmic.BuildingBlocks.Domain.Results;

            public static class SampleErrors
            {
                public static readonly Error Busy = {|#0:Error.Unavailable("Sample.Busy", "繁忙")|};
            }
            """;

        await Verify.VerifyAsync(Source, Verify.ModuleDomain, Verify.Diagnostic(ErrorCatalogAnalyzer.UnavailableOutsideFramework, 0));
    }

    [Fact]
    public async Task Factory_InTestProject_NoDiagnostic()
    {
        const string Source = """
            using Kmic.BuildingBlocks.Domain.Results;

            public sealed class ResultTests
            {
                private static readonly Error _rule = Error.BusinessRule("Test.Rule", "违反规则");
            }
            """;

        await Verify.VerifyAsync(Source, "Kmic.Sample.Domain.Tests");
    }
}
