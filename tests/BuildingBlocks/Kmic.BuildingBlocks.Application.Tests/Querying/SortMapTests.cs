using Kmic.BuildingBlocks.Application.Querying;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Querying;

public sealed class SortMapTests
{
    private static readonly SortMap<Item> _map = SortMap<Item>
        .Field("number", x => x.Number)
        .Field("createdAt", x => x.CreatedAt)
        .Field("amount", x => x.Amount)
        .Default("-createdAt");

    private static readonly DateTimeOffset _day = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Fields_AreListedInDeclarationOrder()
    {
        _map.Fields.ShouldBe(["number", "createdAt", "amount"]);
        _map.DefaultSort.ShouldBe("-createdAt");
    }

    [Fact]
    public void NoSort_UsesTheDefault()
    {
        var terms = _map.Parse(null).Value;

        terms.ShouldHaveSingleItem().ShouldBe(new SortTerm("createdAt", Descending: true));
        _map.Parse("  ").Value.ShouldBe(terms);
    }

    [Fact]
    public void Parse_ReadsDirectionAndOrder()
    {
        var terms = _map.Parse("-amount,number").Value;

        terms.ShouldBe([new SortTerm("amount", Descending: true), new SortTerm("number", Descending: false)]);
    }

    [Theory]
    [InlineData("secret", "不支持按“secret”排序")]
    [InlineData("Number", "不支持按“Number”排序")]
    [InlineData("number,,amount", "不支持按“”排序")]
    [InlineData("number,number", "排序字段“number”重复")]
    [InlineData("number,-number", "排序字段“number”重复")]
    [InlineData("number,amount,createdAt,id", "最多按 3 个字段排序")]
    [InlineData("--number", "不支持按“-number”排序")]
    public void InvalidSort_IsRejectedWithThePathSort(string sort, string message)
    {
        var result = _map.Parse(sort);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.ValidationFailed");
        var error = result.Error.FieldErrors.ShouldHaveSingleItem();
        error.Path.ShouldBe("sort");
        error.Message.ShouldContain(message);
    }

    [Fact]
    public void UnknownField_MessageListsTheAvailableOnes() =>
        _map.Parse("secret").Error.FieldErrors[0].Message.ShouldContain("number、createdAt、amount");

    [Fact]
    public void Apply_OrdersByTheTermsThenById()
    {
        var items = new[]
        {
            new Item(Guid.Parse("00000000-0000-0000-0000-000000000003"), "B", _day, 10),
            new Item(Guid.Parse("00000000-0000-0000-0000-000000000001"), "A", _day, 10),
            new Item(Guid.Parse("00000000-0000-0000-0000-000000000002"), "C", _day, 20),
        }.AsQueryable();

        var byAmountDescending = _map.Apply(items, _map.Parse("-amount").Value).Select(x => x.Number).ToList();
        var byAmountAscending = _map.Apply(items, _map.Parse("amount").Value).Select(x => x.Number).ToList();

        // 金额相同的两条按 Id 升序排在一起，不论金额是升序还是降序
        byAmountDescending.ShouldBe(["C", "A", "B"]);
        byAmountAscending.ShouldBe(["A", "B", "C"]);
    }

    [Fact]
    public void Apply_UsesEverySortKeyInTurn()
    {
        var items = new[]
        {
            new Item(Guid.CreateVersion7(), "B", _day, 10),
            new Item(Guid.CreateVersion7(), "A", _day, 10),
            new Item(Guid.CreateVersion7(), "A", _day.AddDays(1), 5),
        }.AsQueryable();

        var sorted = _map.Apply(items, _map.Parse("-createdAt,number").Value).ToList().Select(x => (x.Number, x.Amount)).ToList();

        sorted.ShouldBe([("A", 5m), ("A", 10m), ("B", 10m)]);
    }

    [Fact]
    public void DefaultSortWithAnUnknownField_FailsAtDeclaration()
    {
        var thrown = Should.Throw<DeclarationException>(() => SortMap<Item>.Field("number", x => x.Number).Default("-nope"));

        thrown.Message.ShouldContain("默认排序");
    }

    [Fact]
    public void ListItemWithoutAnId_FailsAtDeclaration()
    {
        var thrown = Should.Throw<DeclarationException>(() => SortMap<NoId>.Field("name", x => x.Name).Default("name"));

        thrown.Message.ShouldContain("Id");
    }

    [Fact]
    public void FieldDeclaredTwice_FailsAtDeclaration() =>
        Should.Throw<DeclarationException>(() => SortMap<Item>.Field("number", x => x.Number).Field("number", x => x.Amount));

    [Theory]
    [InlineData("-number")]
    [InlineData("a,b")]
    public void FieldNameThatCannotBeWrittenInASortExpression_FailsAtDeclaration(string name) =>
        Should.Throw<DeclarationException>(() => SortMap<Item>.Field(name, x => x.Number));

    private sealed record Item(Guid Id, string Number, DateTimeOffset CreatedAt, decimal Amount);

    private sealed record NoId(string Name);
}
