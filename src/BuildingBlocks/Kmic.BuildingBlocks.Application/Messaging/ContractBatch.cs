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
    /// 把 ID 去重后按 <paramref name="size"/> 分批查询，合并成一个以 ID 为键的字典
    /// </summary>
    /// <remarks>
    /// 提供方的适配器用它：调用方传多少个 ID 都可以，超过一次的上限就自动分批，每批一个查询，按顺序执行，
    /// 不会同时占用多条连接。没有查到的 ID 不会出现在结果里；某一批失败就停下，把这个失败原样返回
    /// </remarks>
    /// <param name="ids">要查的 ID，可以重复，可以为空</param>
    /// <param name="size">每批最多多少个，不能超过提供方查询接受的上限</param>
    /// <param name="fetch">查一批，通常是发送本模块的 Contracts 查询</param>
    /// <param name="ct">取消令牌</param>
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
