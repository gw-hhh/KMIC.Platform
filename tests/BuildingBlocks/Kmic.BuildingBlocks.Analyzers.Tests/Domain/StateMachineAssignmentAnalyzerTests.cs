using Kmic.BuildingBlocks.Analyzers.Domain;
using Microsoft.CodeAnalysis.Testing;
using Verify = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.Domain.StateMachineAssignmentAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Domain;

/// <summary>
/// 场景来自原型 #8，改为引用真实的状态机类型（Define 的第一个参数是表达式树）
/// </summary>
public sealed class StateMachineAssignmentAnalyzerTests
{
    private const string StandardDefine = "(s => s.Status, (s, v) => s.Status = v)";
    private const string StandardStatus = "public SampleStatus Status { get; private set; }";

    [Fact]
    public async Task Assignment_InsideDefineSetter_NoDiagnostic()
    {
        var source = Aggregate("""
            public void Rename(string title) => Title = title;

            public Result Submit() => _machine.Fire(this, SampleAction.Submit, null);
            """) + """

            public sealed class Unrelated
            {
                public SampleStatus Status { get; set; }

                public void Reset() => Status = SampleStatus.Draft;
            }
            """;

        await Verify.VerifyAsync(source, Verify.ModuleDomain);
    }

    [Fact]
    public async Task Assignment_InMethodConstructorOrInitializer_Reports()
    {
        var source = Aggregate("""
            private Sample() : base(Guid.CreateVersion7())
            {
                {|#0:Status|} = SampleStatus.Draft;
            }

            public void Approve() => {|#1:this.Status|} = SampleStatus.Approved;

            public static Sample CreateSubmitted() => new Sample { {|#2:Status|} = SampleStatus.Submitted };
            """);

        await Verify.VerifyAsync(source, Verify.ModuleDomain, Direct(0), Direct(1), Direct(2));
    }

    [Fact]
    public async Task PropertyInitializer_CompoundOrDeconstruction_Reports()
    {
        var source = Aggregate(
            """
            public void Advance()
            {
                {|#1:Status|}++;
                ({|#2:Status|}, Title) = (SampleStatus.Approved, "示例");
            }
            """,
            status: "public SampleStatus Status { get; private set; } {|#0:= SampleStatus.Draft|};");

        await Verify.VerifyAsync(source, Verify.ModuleDomain, Direct(0), Direct(1), Direct(2));
    }

    [Fact]
    public async Task PlaceholderInitializer_NoDiagnostic()
    {
        // 智能枚举（引用类型）的状态属性写 = null!，普通枚举写 = default!，都只是给 EF 物化用的占位
        var plainEnum = Aggregate(string.Empty, status: "public SampleStatus Status { get; private set; } = default!;");
        const string SmartEnum = """
            using System;
            using Kmic.BuildingBlocks.Domain.Entities;
            using Kmic.BuildingBlocks.Domain.Enumerations;
            using Kmic.BuildingBlocks.Domain.StateMachines;

            public sealed class OrderStatus : Enumeration<OrderStatus>
            {
                public static readonly OrderStatus Draft = new("Draft", "草稿");

                private OrderStatus(string code, string name) : base(code, name) { }
            }

            public enum OrderAction { Submit }

            public sealed class Order : AggregateRoot<Guid>
            {
                private static readonly StateMachine<Order, OrderStatus, OrderAction> _machine =
                    StateMachine.Define<Order, OrderStatus, OrderAction>(o => o.Status, (o, v) => o.Status = v).Build();

                public OrderStatus Status { get; private set; } = null!;
            }
            """;

        await Verify.VerifyAsync(plainEnum, Verify.ModuleDomain);
        await Verify.VerifyAsync(SmartEnum, Verify.ModuleDomain);
    }

    [Fact]
    public async Task Assignment_InOtherLambda_Reports()
    {
        var source = Aggregate("public Action ApproveLater() => () => {|#0:Status|} = SampleStatus.Approved;");

        await Verify.VerifyAsync(source, Verify.ModuleDomain, Direct(0));
    }

    [Theory]
    [InlineData("set")]
    [InlineData("internal set")]
    public async Task Setter_NotPrivate_Reports(string setter)
    {
        var source = Aggregate(string.Empty, status: $$"""public SampleStatus {|#0:Status|} { get; {{setter}}; }""");

        await Verify.VerifyAsync(source, Verify.ModuleDomain, Verify.Diagnostic(StateMachineAssignmentAnalyzer.SetterNotPrivate, 0, "Status"));
    }

    [Fact]
    public async Task Property_NotAuto_Reports()
    {
        var source = Aggregate(
            string.Empty,
            status: "private SampleStatus _status; public SampleStatus {|#0:Status|} { get => _status; private set => _status = value; }");

        await Verify.VerifyAsync(source, Verify.ModuleDomain, Verify.Diagnostic(StateMachineAssignmentAnalyzer.NotAutoProperty, 0, "Status"));
    }

    [Fact]
    public async Task Define_GetterOrSetterNotPlain_Reports()
    {
        var getter = Aggregate(string.Empty, define: "({|#0:s => s.Title.Length > 0 ? s.Status : SampleStatus.Draft|}, (s, v) => s.Status = v)");
        var setter = Aggregate(string.Empty, define: """(s => s.Status, {|#0:(s, v) => { s.Status = v; s.Title = "已改"; }|})""");

        await Verify.VerifyAsync(getter, Verify.ModuleDomain, Verify.Diagnostic(StateMachineAssignmentAnalyzer.InvalidGetter, 0));
        await Verify.VerifyAsync(setter, Verify.ModuleDomain, Verify.Diagnostic(StateMachineAssignmentAnalyzer.InvalidSetter, 0));
    }

    [Fact]
    public async Task PartialClassOrNestedType_Reports()
    {
        var source = Aggregate("""
            private sealed class Fixer
            {
                public void Fix(Sample sample) => {|#1:sample.Status|} = SampleStatus.Approved;
            }
            """);
        const string Other = """
            public sealed partial class Sample
            {
                public void Approve() => {|#0:Status|} = SampleStatus.Approved;
            }
            """;

        await Verify.VerifyAsync(
            [source, Other],
            Verify.ModuleDomain,
            Direct(0),
            Verify.Diagnostic(StateMachineAssignmentAnalyzer.AssignmentOutsideAggregate, 1, "Status"));
    }

    private static DiagnosticResult Direct(int location) =>
        Verify.Diagnostic(StateMachineAssignmentAnalyzer.DirectAssignment, location, "Status");

    private static string Aggregate(string members, string define = StandardDefine, string status = StandardStatus) => $$"""
        using System;
        using Kmic.BuildingBlocks.Domain.Entities;
        using Kmic.BuildingBlocks.Domain.Results;
        using Kmic.BuildingBlocks.Domain.StateMachines;

        public enum SampleStatus { Draft, Submitted, Approved }

        public enum SampleAction { Submit, Approve }

        public sealed partial class Sample : AggregateRoot<Guid>
        {
            private static readonly StateMachine<Sample, SampleStatus, SampleAction> _machine =
                StateMachine.Define<Sample, SampleStatus, SampleAction>{{define}}.Build();

            {{status}}

            public string Title { get; private set; } = string.Empty;

            {{members}}
        }
        """;
}
