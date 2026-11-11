using System.Linq.Expressions;
using Kmic.BuildingBlocks.Application.Querying;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Querying;

/// <summary>
/// 排序键要能还原到投影之前的表达式：EF 翻译不了对构造函数投影出的对象成员排序，所以排序放在投影之前
/// </summary>
public sealed class ProjectionOrderingTests
{
    private static readonly Row[] _rows =
    [
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "b", 20),
        new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "a", 20),
        new(Guid.Parse("00000000-0000-0000-0000-000000000003"), "c", 10),
    ];

    private static readonly SortMap<ListItem> _map = SortMap<ListItem>
        .Field("label", x => x.Label)
        .Field("total", x => x.Total)
        .Default("label");

    [Fact]
    public void PrimaryConstructorProjection_IsSortedThroughTheProjection()
    {
        var query = _rows.AsQueryable().Select(r => new ListItem(r.Id, r.Name.ToUpperInvariant(), r.Amount * 2));

        _map.Apply(query, _map.Parse("-total,label").Value).Select(x => x.Label).ToList().ShouldBe(["A", "B", "C"]);
    }

    [Fact]
    public void Sorting_HappensBeforeTheProjection()
    {
        // 排序在投影之前执行，查询的最后一步仍然是投影
        var query = _rows.AsQueryable().Select(r => new ListItem(r.Id, Marker(r.Name), r.Amount));

        var sorted = _map.Apply(query, _map.Parse("label").Value).ToList();

        sorted.Select(x => x.Label).ShouldBe(["a!", "b!", "c!"]);
        ((MethodCallExpression)_map.Apply(query, _map.Parse("label").Value).Expression).Method.Name.ShouldBe("Select");
    }

    [Fact]
    public void MemberInitProjection_IsSortedThroughTheProjection()
    {
        var query = _rows.AsQueryable().Select(r => new Initialized { Id = r.Id, Label = r.Name, Total = r.Amount });
        var map = SortMap<Initialized>.Field("label", x => x.Label).Default("-label");

        map.Apply(query, map.Parse(null).Value).Select(x => x.Label).ToList().ShouldBe(["c", "b", "a"]);
    }

    [Fact]
    public void AnonymousProjection_IsSortedThroughTheProjection()
    {
        var query = _rows.AsQueryable().Select(r => new { r.Id, Label = r.Name, Total = r.Amount });
        var map = SortMap<Anonymous>.Field("label", x => x.Label).Default("label");

        map.Apply(query.Select(x => new Anonymous(x.Id, x.Label)), map.Parse("-label").Value).Select(x => x.Label).ToList().ShouldBe(["c", "b", "a"]);
    }

    [Fact]
    public void QueryWithoutAProjection_IsSortedDirectly()
    {
        var plain = SortMap<Row>.Field("name", x => x.Name).Default("name");

        plain.Apply(_rows.AsQueryable(), plain.Parse("-name").Value).Select(x => x.Name).ToList().ShouldBe(["c", "b", "a"]);
    }

    [Fact]
    public void ProjectionKeepsItsOwnCapturedValues()
    {
        var suffix = "-x";
        var query = _rows.AsQueryable().Select(r => new ListItem(r.Id, r.Name + suffix, r.Amount));

        _map.Apply(query, _map.Parse("-label").Value).Select(x => x.Label).ToList().ShouldBe(["c-x", "b-x", "a-x"]);
    }

    private static string Marker(string name) => name + "!";

    private sealed record Row(Guid Id, string Name, decimal Amount);

    private sealed record ListItem(Guid Id, string Label, decimal Total);

    private sealed record Anonymous(Guid Id, string Label);

    private sealed class Initialized
    {
        public Guid Id { get; init; }

        public string Label { get; init; } = "";

        public decimal Total { get; init; }
    }
}
