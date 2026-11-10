using Kmic.BuildingBlocks.Analyzers.Domain;
using Verify = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.Domain.EnumerationAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Domain;

public sealed class EnumerationAnalyzerTests
{
    [Fact]
    public async Task StandardEnumeration_NoDiagnostic()
    {
        const string Source = """
            using Kmic.BuildingBlocks.Domain.Enumerations;

            public sealed class SampleStatus : Enumeration<SampleStatus>
            {
                public static readonly SampleStatus Draft = new("Draft", "草稿", editable: true);
                public static readonly SampleStatus Submitted = new("Submitted", "已提交", editable: false);

                private SampleStatus(string code, string name, bool editable) : base(code, name) => Editable = editable;

                public bool Editable { get; }
            }
            """;

        await Verify.VerifyAsync(Source, Verify.ModuleDomain);
    }

    [Fact]
    public async Task WrongShape_Reports()
    {
        const string Source = """
            using Kmic.BuildingBlocks.Domain.Enumerations;

            public class {|#0:SampleStatus|} : Enumeration<SampleStatus>
            {
                public static SampleStatus {|#1:Draft|} = new("Draft", "草稿");

                public {|#2:SampleStatus|}(string code, string name) : base(code, name) { }
            }
            """;

        await Verify.VerifyAsync(
            Source,
            Verify.ModuleDomain,
            Verify.Diagnostic(EnumerationAnalyzer.NotSealed, 0, "SampleStatus"),
            Verify.Diagnostic(EnumerationAnalyzer.MutableValue, 1, "Draft"),
            Verify.Diagnostic(EnumerationAnalyzer.NonPrivateConstructor, 2, "SampleStatus"));
    }
}
