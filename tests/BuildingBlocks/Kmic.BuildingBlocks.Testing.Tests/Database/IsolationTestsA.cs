using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Database;

/// <summary>
/// 与 IsolationTestsB 并行运行：两个类各有自己的克隆库，互相看不到对方写入的行
/// </summary>
public sealed class IsolationTestsA(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    [Fact]
    public async Task Insert_OnlyOwnRowsVisible()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await ProbeRows.InsertAsync(database.DataSource, nameof(IsolationTestsA), 3, cancellationToken);

        (await ProbeRows.OwnersAsync(database.DataSource, cancellationToken)).ShouldBe(Enumerable.Repeat(nameof(IsolationTestsA), 3));
    }
}
