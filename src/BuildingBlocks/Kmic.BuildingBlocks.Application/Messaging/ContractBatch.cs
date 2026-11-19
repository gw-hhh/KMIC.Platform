using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// Contracts 批量查询的辅助方法
/// </summary>
public static class ContractBatch
{
    /// <summary>
    /// 一次调用最多带多少个 ID
    /// </summary>
    public const int DefaultSize = 1000;

    /// <summary>
    /// 按 ID 去重后分批查询，合并成以 ID 为键的字典
    /// </summary>
    /// <remarks>
    /// 没查到的 ID 不在结果里
    /// </remarks>
    /// <param name="ids">要查的 ID，可以重复，可以为空</param>
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
