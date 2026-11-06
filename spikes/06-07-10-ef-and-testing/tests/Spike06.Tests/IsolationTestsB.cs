using Xunit;

namespace Spike06.Tests;

public sealed class IsolationTestsB(ClassDatabase db) : IClassFixture<ClassDatabase>
{
    [Fact]
    public Task Class_sees_only_its_own_rows() => IsolationCheck.RunAsync(db, "B");
}
