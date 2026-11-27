using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Database;

// 与 IsolationTestsA 并行运行，验证克隆库隔离
public sealed class IsolationTestsB(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    [Fact]
    public async Task Insert_OnlyOwnRowsVisible()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await ProbeRows.InsertAsync(database.DataSource, nameof(IsolationTestsB), 2, cancellationToken);

        (await ProbeRows.OwnersAsync(database.DataSource, cancellationToken)).ShouldBe(Enumerable.Repeat(nameof(IsolationTestsB), 2));
    }
}
