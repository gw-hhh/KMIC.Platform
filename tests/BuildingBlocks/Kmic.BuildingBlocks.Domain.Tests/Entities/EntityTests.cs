using Kmic.BuildingBlocks.Domain.Entities;
using Shouldly;

namespace Kmic.BuildingBlocks.Domain.Tests.Entities;

public sealed class EntityTests
{
    [Fact]
    public void Equals_SameTypeAndId_True()
    {
        var id = Guid.CreateVersion7();
        var left = new Line(id);
        var right = new Line(id);

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentTypeSameId_False()
    {
        var id = Guid.CreateVersion7();

        Entity<Guid> line = new Line(id);
        Entity<Guid> note = new Note(id);

        line.Equals(note).ShouldBeFalse();
        (line == note).ShouldBeFalse();
    }

    [Fact]
    public void Equals_Null_False()
    {
        var line = new Line(Guid.CreateVersion7());

        line.Equals(null).ShouldBeFalse();
        (line == null).ShouldBeFalse();
    }

    [Fact]
    public void DictionaryKey_UsesIdentity()
    {
        var id = Guid.CreateVersion7();
        var map = new Dictionary<Line, string> { [new Line(id)] = "第一行" };

        map[new Line(id)].ShouldBe("第一行");
    }

    private sealed class Line(Guid id) : Entity<Guid>(id);

    private sealed class Note(Guid id) : Entity<Guid>(id);
}
