using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Database;

/// <summary>
/// 和 IsolationTestsA 并行运行
/// </summary>
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
