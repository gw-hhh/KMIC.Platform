using Kmic.BuildingBlocks.Testing.Waiting;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Waiting;

public sealed class EventuallyTests
{
    [Fact]
    public async Task UntilAsync_ReturnsFirstSatisfyingProbe()
    {
        var calls = 0;

        var value = await Eventually.UntilAsync(
            _ => Task.FromResult(++calls),
            count => count >= 3,
            "第三次读取",
            ct: TestContext.Current.CancellationToken);

        value.ShouldBe(3);
        calls.ShouldBe(3);
    }

    [Fact]
    public async Task UntilAsync_Timeout_ReportsWhatWasAwaited()
    {
        var thrown = await Should.ThrowAsync<TimeoutException>(() => Eventually.UntilAsync(
            _ => Task.FromResult(false),
            "永远不会完成的条件",
            TimeSpan.FromMilliseconds(120),
            TestContext.Current.CancellationToken));

        thrown.Message.ShouldContain("永远不会完成的条件");
    }
}
