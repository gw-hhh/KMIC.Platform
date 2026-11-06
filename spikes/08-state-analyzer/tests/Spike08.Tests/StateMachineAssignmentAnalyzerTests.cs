using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Spike08.Analyzers;
using Xunit;

namespace Spike08.Tests;

public sealed class StateMachineAssignmentAnalyzerTests
{
    // 只保留分析器识别所需的签名
    private const string StateMachineSource = """
        using System;

        namespace Kmic.BuildingBlocks.Domain
        {
            public static class StateMachine
            {
                public static StateMachine<TAggregate, TState, TAction> Define<TAggregate, TState, TAction>(
                    Func<TAggregate, TState> getter,
                    Action<TAggregate, TState> setter) => new();
            }

            public sealed class StateMachine<TAggregate, TState, TAction>
            {
                public StateMachine<TAggregate, TState, TAction> Permit(TState from, TAction action, TState to) => this;

                public StateMachine<TAggregate, TState, TAction> Build() => this;

                public void Fire(TAggregate aggregate, TAction action) { }
            }
        }
        """;

    private const string StandardDefine = "(s => s.Status, (s, v) => s.Status = v)";
    private const string StandardStatus = "public SampleStatus Status { get; private set; }";

    [Fact]
    public async Task Assignment_InsideDefineSetter_NoDiagnostic()
    {
        var source = Aggregate("""
            public void Rename(string title) => Title = title;

            public void Submit() => _machine.Fire(this, SampleAction.Submit);
            """) + """

            namespace Sample
            {
                public sealed class Unrelated
                {
                    public SampleStatus Status { get; set; }

                    public void Reset() => Status = SampleStatus.Draft;
                }
            }
            """;

        await VerifyAsync([source]);
    }

    [Fact]
    public async Task Assignment_InMethod_Reports()
    {
        var source = Aggregate("""
            public void Approve()
            {
                {|#0:Status|} = SampleStatus.Approved;
            }

            public void ResetToDraft() => {|#1:this.Status|} = SampleStatus.Draft;
            """);

        await VerifyAsync([source], DirectAssignment(0, "Status"), DirectAssignment(1, "Status"));
    }

    [Fact]
    public async Task Assignment_InConstructorOrObjectInitializer_Reports()
    {
        var source = Aggregate("""
            public Sample()
            {
                {|#0:Status|} = SampleStatus.Draft;
            }

            public static Sample CreateSubmitted() => new Sample { {|#1:Status|} = SampleStatus.Submitted };
            """);

        await VerifyAsync([source], DirectAssignment(0, "Status"), DirectAssignment(1, "Status"));
    }

    [Fact]
    public async Task PropertyInitializer_Reports()
    {
        var source = Aggregate(string.Empty, status: "public SampleStatus Status { get; private set; } {|#0:= SampleStatus.Draft|};");

        await VerifyAsync([source], DirectAssignment(0, "Status"));
    }

    [Fact]
    public async Task CompoundIncrementOrDeconstruction_Reports()
    {
        var source = Aggregate("""
            public void Advance()
            {
                {|#0:Status|}++;
                {|#1:Status|} += 1;
                ({|#2:Status|}, Title) = (SampleStatus.Approved, "示例");
            }
            """);

        await VerifyAsync([source], DirectAssignment(0, "Status"), DirectAssignment(1, "Status"), DirectAssignment(2, "Status"));
    }

    [Fact]
    public async Task Assignment_InOtherLambda_Reports()
    {
        // 只有 Define 的 setter 可以赋值，其他 lambda 不行
        var source = Aggregate("""
            public System.Action ApproveLater() => () => {|#0:Status|} = SampleStatus.Approved;
            """);

        await VerifyAsync([source], DirectAssignment(0, "Status"));
    }

    [Theory]
    [InlineData("set")]
    [InlineData("internal set")]
    public async Task Setter_NotPrivate_Reports(string setter)
    {
        var source = Aggregate(string.Empty, status: $$"""public SampleStatus {|#0:Status|} { get; {{setter}}; }""");

        await VerifyAsync([source], Expect(StateMachineAssignmentAnalyzer.SetterNotPrivate, 0, "Status"));
    }

    [Theory]
    [InlineData("private SampleStatus _status; public SampleStatus {|#0:Status|} { get => _status; private set => _status = value; }")]
    [InlineData("public SampleStatus {|#0:Status|} { get; private set => field = value; }")]
    public async Task Property_NotAuto_Reports(string status)
    {
        var source = Aggregate(string.Empty, status: status);

        await VerifyAsync([source], Expect(StateMachineAssignmentAnalyzer.NotAutoProperty, 0, "Status"));
    }

    [Fact]
    public async Task Define_GetterNotPlainProperty_Reports()
    {
        var source = Aggregate(
            "public void Approve() => Status = SampleStatus.Approved;",
            define: "({|#0:s => s.Title.Length > 0 ? s.Status : SampleStatus.Draft|}, (s, v) => s.Status = v)");

        await VerifyAsync([source], Expect(StateMachineAssignmentAnalyzer.InvalidGetter, 0));
    }

    [Fact]
    public async Task Define_SetterDoesMoreThanAssign_Reports()
    {
        var source = Aggregate(
            string.Empty,
            define: """(s => s.Status, {|#0:(s, v) => { s.Status = v; s.Title = "已改"; }|})""");

        await VerifyAsync([source], Expect(StateMachineAssignmentAnalyzer.InvalidSetter, 0));
    }

    [Fact]
    public async Task TwoStateProperties_EachChecked()
    {
        var source = Aggregate("""
            private static readonly StateMachine<Sample, PaymentStatus, SampleAction> _paymentMachine =
                StateMachine.Define<Sample, PaymentStatus, SampleAction>(s => s.Payment, (s, v) => s.Payment = v).Build();

            public PaymentStatus Payment { get; private set; }

            public void MarkPaid() => {|#0:Payment|} = PaymentStatus.Paid;

            public void Approve() => {|#1:Status|} = SampleStatus.Approved;
            """);

        await VerifyAsync([source], DirectAssignment(0, "Payment"), DirectAssignment(1, "Status"));
    }

    [Fact]
    public async Task PartialClass_AssignmentInOtherFile_Reports()
    {
        var other = """
            namespace Sample
            {
                public sealed partial class Sample
                {
                    public void Approve() => {|#0:Status|} = SampleStatus.Approved;
                }
            }
            """;

        await VerifyAsync([Aggregate(string.Empty), other], DirectAssignment(0, "Status"));
    }

    [Fact]
    public async Task NestedType_Assignment_Reports()
    {
        var source = Aggregate("""
            private sealed class Fixer
            {
                public void Fix(Sample sample) => {|#0:sample.Status|} = SampleStatus.Approved;
            }
            """);

        await VerifyAsync([source], Expect(StateMachineAssignmentAnalyzer.AssignmentOutsideAggregate, 0, "Status"));
    }

    [Fact]
    public async Task OtherType_AssignsNonPrivateSetter_ReportsBoth()
    {
        var source = Aggregate(string.Empty, status: "public SampleStatus {|#0:Status|} { get; set; }") + """

            namespace Sample
            {
                public sealed class Approver
                {
                    public void Approve(Sample sample) => {|#1:sample.Status|} = SampleStatus.Approved;
                }
            }
            """;

        await VerifyAsync(
            [source],
            Expect(StateMachineAssignmentAnalyzer.SetterNotPrivate, 0, "Status"),
            Expect(StateMachineAssignmentAnalyzer.AssignmentOutsideAggregate, 1, "Status"));
    }

    [Fact]
    public async Task WithoutStateMachineType_NoDiagnostic()
    {
        var source = """
            namespace Sample
            {
                public enum SampleStatus { Draft, Approved }

                public sealed class Sample
                {
                    public SampleStatus Status { get; private set; }

                    public void Approve() => Status = SampleStatus.Approved;
                }
            }
            """;

        var test = new CSharpAnalyzerTest<StateMachineAssignmentAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
        };
        test.TestState.Sources.Add(source);

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    private static string Aggregate(string members, string define = StandardDefine, string status = StandardStatus) => $$"""
        using Kmic.BuildingBlocks.Domain;

        namespace Sample
        {
            public enum SampleStatus { Draft, Submitted, Approved }

            public enum PaymentStatus { Unpaid, Paid }

            public enum SampleAction { Submit, Approve }

            public sealed partial class Sample
            {
                private static readonly StateMachine<Sample, SampleStatus, SampleAction> _machine =
                    StateMachine.Define<Sample, SampleStatus, SampleAction>{{define}}
                        .Permit(SampleStatus.Draft, SampleAction.Submit, SampleStatus.Submitted)
                        .Build();

                {{status}}

                public string Title { get; private set; } = string.Empty;

                {{members}}
            }
        }
        """;

    private static DiagnosticResult DirectAssignment(int location, string property) =>
        Expect(StateMachineAssignmentAnalyzer.DirectAssignment, location, property);

    // KM0401 有多种提示，按位置标记并写明期望哪一种
    private static DiagnosticResult Expect(DiagnosticDescriptor descriptor, int location, params object[] arguments) =>
        new DiagnosticResult(descriptor).WithLocation(location).WithArguments(arguments);

    private static async Task VerifyAsync(string[] sources, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<StateMachineAssignmentAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
        };
        test.TestState.Sources.Add(StateMachineSource);
        foreach (var source in sources)
        {
            test.TestState.Sources.Add(source);
        }

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
