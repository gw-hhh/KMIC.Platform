using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>Contracts 批量查询辅助工具。</summary>
public static class ContractBatch
{
    /// <summary>单次批量查询默认最大 ID 数量。</summary>
    public const int DefaultSize = 1000;

    /// <summary>对 <paramref name="ids"/> 去重后分批查询并合并为字典；未命中的 ID 不包含在结果中。</summary>
    public static async Task<Result<IReadOnlyDictionary<TKey, TValue>>> ChunkedAsync<TKey, TValue>(
        IReadOnlyCollection<TKey> ids,
        int size,
        Func<IReadOnlyList<TKey>, CancellationToken, ValueTask<Result<IReadOnlyDictionary<TKey, TValue>>>> fetch,
        CancellationToken ct)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(fetch);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        var merged = new Dictionary<TKey, TValue>();
        foreach (var batch in ids.Distinct().Chunk(size))
        {
            var found = await fetch(batch, ct);
            if (found.IsFailure)
            {
                return found.Error;
            }

            foreach (var (key, value) in found.Value)
            {
                merged[key] = value;
            }
        }

        return merged;
    }
}
