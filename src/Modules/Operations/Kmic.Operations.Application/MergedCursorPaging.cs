using System.Globalization;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application;

/// <summary>
/// 跨模块游标分页
/// </summary>
/// <remarks>
/// 各模块查询和合并结果均按时间、ID 倒序排列
/// </remarks>
internal static class MergedCursorPaging
{
    /// <summary>
    /// 合并各模块查询结果并生成下一页游标
    /// </summary>
    /// <remarks>
    /// fetch 须按时间、ID 倒序查询，并应用传入的游标和数量限制。
    /// ID 排序须与 PostgreSQL 的 uuid 排序规则一致。
    /// </remarks>
    public static async Task<Result<(IReadOnlyList<T> Items, string? NextCursor)>> PageAsync<TStore, T>(
        IEnumerable<TStore> stores,
        CursorRequest request,
        ICursorProtector protector,
        Func<TStore, (DateTimeOffset At, Guid Id)?, int, Task<IReadOnlyList<T>>> fetch,
        Func<T, DateTimeOffset> timeOf,
        Func<T, Guid> idOf)
    {
        if (request.Limit is < 1 or > CursorRequest.MaxLimit)
        {
            return CommonErrors.ValidationFailedWith([new FieldError("limit", "Range", $"每页数量必须在 1 到 {CursorRequest.MaxLimit} 之间")]);
        }

        (DateTimeOffset At, Guid Id)? after = null;
        if (request.After is { Length: > 0 } cursor)
        {
            if (!protector.TryUnprotect(cursor, out var key, out var id)
                || !DateTimeOffset.TryParseExact(key, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var at))
            {
                return CommonErrors.ValidationFailedWith([new FieldError("after", "InvalidCursor", "游标无效或已被修改")]);
            }

            after = (at, id);
        }

        var take = request.Limit + 1;
        var batches = await Task.WhenAll(stores.Select(store => fetch(store, after, take)));
        var merged = batches
            .SelectMany(batch => batch)
            .OrderByDescending(timeOf)
            .ThenByDescending(idOf, DatabaseUuidOrder.Instance)
            .Take(take)
            .ToList();
        var hasMore = merged.Count > request.Limit;
        IReadOnlyList<T> items = hasMore ? merged.Take(request.Limit).ToList() : merged;
        var next = hasMore ? protector.Protect(timeOf(items[^1]).ToString("O", CultureInfo.InvariantCulture), idOf(items[^1])) : null;
        return (items, next);
    }

    // 按大端字节序比较 UUID，使内存归并与 PostgreSQL 查询使用相同顺序
    private sealed class DatabaseUuidOrder : IComparer<Guid>
    {
        public static DatabaseUuidOrder Instance { get; } = new();

        public int Compare(Guid x, Guid y)
        {
            Span<byte> left = stackalloc byte[16];
            Span<byte> right = stackalloc byte[16];
            x.TryWriteBytes(left, bigEndian: true, out _);
            y.TryWriteBytes(right, bigEndian: true, out _);
            return left.SequenceCompareTo(right);
        }
    }
}
