using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Messaging;

/// <summary>
/// 批量查询的分批与合并
/// </summary>
public sealed class ContractBatchTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ChunkedAsync_SplitsIntoBatchesOfAtMostTheSize_InOrder()
    {
        var batches = new List<int[]>();

        var result = await ContractBatch.ChunkedAsync<int, string>(Enumerable.Range(1, 7).ToArray(), 3, (batch, _) => FoundAsync(batches, batch), Ct);

        batches.ShouldBe([[1, 2, 3], [4, 5, 6], [7]]);
        result.Value.Keys.ShouldBe([1, 2, 3, 4, 5, 6, 7], ignoreOrder: true);
        result.Value[5].ShouldBe("#5");
    }

    [Fact]
    public async Task ChunkedAsync_DuplicateIds_QueriedOnce()
    {
        var batches = new List<int[]>();

        var result = await ContractBatch.ChunkedAsync<int, string>([3, 1, 3, 3, 1], 10, (batch, _) => FoundAsync(batches, batch), Ct);

        batches.ShouldBe([[3, 1]]);
        result.Value.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ChunkedAsync_NoIds_NothingFetched()
    {
        var batches = new List<int[]>();

        var result = await ContractBatch.ChunkedAsync<int, string>([], 10, (batch, _) => FoundAsync(batches, batch), Ct);

        batches.ShouldBeEmpty();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ChunkedAsync_IdsTheProviderDoesNotKnow_AbsentFromTheResult()
    {
        var result = await ContractBatch.ChunkedAsync<int, string>(
            [1, 2, 3, 4],
            2,
            (batch, _) => ValueTask.FromResult<Result<IReadOnlyDictionary<int, string>>>(batch.Where(id => id % 2 == 0).ToDictionary(id => id, id => $"#{id}")),
            Ct);

        result.Value.Keys.ShouldBe([2, 4], ignoreOrder: true);
    }

    [Fact]
    public async Task ChunkedAsync_FirstFailure_StopsAndIsReturnedAsIs()
    {
        var error = Error.BusinessRule("Test.Failed", "失败");
        var calls = 0;

        var result = await ContractBatch.ChunkedAsync<int, string>(
            Enumerable.Range(1, 9).ToArray(),
            3,
            (_, _) =>
            {
                calls++;
                return ValueTask.FromResult<Result<IReadOnlyDictionary<int, string>>>(calls == 2 ? error : new Dictionary<int, string>());
            },
            Ct);

        result.Error.ShouldBeSameAs(error);
        calls.ShouldBe(2);
    }

    [Fact]
    public async Task ChunkedAsync_CancellationToken_PassedToEveryBatch()
    {
        using var source = new CancellationTokenSource();
        var seen = new List<CancellationToken>();

        await ContractBatch.ChunkedAsync<int, string>(
            [1, 2, 3],
            1,
            (batch, token) =>
            {
                seen.Add(token);
                return FoundAsync([], batch);
            },
            source.Token);

        seen.Count.ShouldBe(3);
        seen.ShouldAllBe(token => token == source.Token);
    }

    [Fact]
    public async Task ChunkedAsync_SizeBelowOne_Refused() =>
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => ContractBatch.ChunkedAsync<int, string>([1], 0, (batch, _) => FoundAsync([], batch), Ct));

    private static ValueTask<Result<IReadOnlyDictionary<int, string>>> FoundAsync(List<int[]> batches, IReadOnlyList<int> batch)
    {
        batches.Add([.. batch]);
        return ValueTask.FromResult<Result<IReadOnlyDictionary<int, string>>>(batch.ToDictionary(id => id, id => $"#{id}"));
    }
}
