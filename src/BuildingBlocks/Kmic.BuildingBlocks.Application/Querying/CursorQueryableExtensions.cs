using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 游标分页：按“排序键 + ID”的键集往后翻，不数总数，翻页不受并发插入影响
/// </summary>
/// <remarks>
/// 每个游标列表固定一种排序，不接受调用方指定。排序键支持时间（DateTimeOffset、DateTime）与整数（int、long），
/// 排序键相同时按 ID 排，所以结果稳定。游标无效或被改过、数量超出范围返回校验错误。
/// 列表项的 ID 用强类型 ID（Vogen 生成的，带 From(Guid) 与 CompareTo）
/// </remarks>
public static class CursorQueryableExtensions
{
    /// <summary>
    /// 取一页
    /// </summary>
    /// <param name="source">查询，已经带了数据范围过滤和业务筛选，并投影成了列表项</param>
    /// <param name="request">请求参数</param>
    /// <param name="protector">游标编码</param>
    /// <param name="key">排序键</param>
    /// <param name="id">行的 ID，用来在排序键相同时定先后</param>
    /// <param name="descending">是否倒序</param>
    /// <param name="ct">取消令牌</param>
    /// <typeparam name="T">列表项</typeparam>
    /// <typeparam name="TKey">排序键类型</typeparam>
    /// <typeparam name="TId">强类型 ID</typeparam>
    public static Task<Result<CursorPage<T>>> ToCursorPageAsync<T, TKey, TId>(
        this IQueryable<T> source,
        CursorRequest request,
        ICursorProtector protector,
        Expression<Func<T, TKey>> key,
        Expression<Func<T, TId>> id,
        bool descending,
        CancellationToken ct)
        where TKey : struct, IComparable<TKey>, IFormattable
        where TId : struct, IStronglyTypedId, IComparable<TId> =>
        ToCursorPageCoreAsync(source, request, protector, key, id, descending, StronglyTypedIds<TId>.FromGuid, value => value.Value, ct);

    private static async Task<Result<CursorPage<T>>> ToCursorPageCoreAsync<T, TKey, TId>(
        IQueryable<T> source,
        CursorRequest request,
        ICursorProtector protector,
        Expression<Func<T, TKey>> key,
        Expression<Func<T, TId>> id,
        bool descending,
        Func<Guid, TId> fromGuid,
        Func<TId, Guid> toGuid,
        CancellationToken ct)
        where TKey : struct, IComparable<TKey>, IFormattable
        where TId : struct, IComparable<TId>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(protector);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(id);

        if (request.Limit is < 1 or > CursorRequest.MaxLimit)
        {
            return CommonErrors.ValidationFailedWith([new FieldError("limit", "Range", $"每页数量必须在 1 到 {CursorRequest.MaxLimit} 之间")]);
        }

        var query = source;
        if (request.After is { Length: > 0 } after)
        {
            if (!protector.TryUnprotect(after, out var keyText, out var lastId) || !TryParse<TKey>(keyText, out var lastKey))
            {
                return CommonErrors.ValidationFailedWith([new FieldError("after", "InvalidCursor", "游标无效或已被修改")]);
            }

            query = ProjectionOrdering.Where(query, After(key, id, lastKey, fromGuid(lastId), descending));
        }

        var ordered = ProjectionOrdering.Order(query, [new SortKey(key, descending), new SortKey(id, descending)]);
        var rows = await ordered.Take(request.Limit + 1).ToListAsync(ct);
        var hasMore = rows.Count > request.Limit;
        var items = hasMore ? rows.Take(request.Limit).ToList() : rows;
        string? next = null;
        if (hasMore)
        {
            var last = items[^1];
            next = protector.Protect(Format(key.Compile()(last)), toGuid(id.Compile()(last)));
        }

        return new CursorPage<T>(items, next);
    }

    // (key, id) 排在游标行之后：倒序是更小，正序是更大。键相同时比 ID
    private static Expression<Func<T, bool>> After<T, TKey, TId>(Expression<Func<T, TKey>> key, Expression<Func<T, TId>> id, TKey lastKey, TId lastId, bool descending)
        where TKey : struct
        where TId : struct, IComparable<TId>
    {
        var row = Expression.Parameter(typeof(T), "row");
        var rowKey = new ParameterReplacer(row).Visit(key.Body);
        var rowId = new ParameterReplacer(row).Visit(id.Body);
        var keyConstant = Expression.Constant(lastKey, typeof(TKey));
        var idComparison = Expression.Call(rowId, typeof(TId).GetMethod(nameof(IComparable<>.CompareTo), [typeof(TId)])!, Expression.Constant(lastId, typeof(TId)));
        var zero = Expression.Constant(0);

        var keyBeyond = descending ? Expression.LessThan(rowKey, keyConstant) : Expression.GreaterThan(rowKey, keyConstant);
        var idBeyond = descending ? Expression.LessThan(idComparison, zero) : Expression.GreaterThan(idComparison, zero);
        var body = Expression.OrElse(keyBeyond, Expression.AndAlso(Expression.Equal(rowKey, keyConstant), idBeyond));
        return Expression.Lambda<Func<T, bool>>(body, row);
    }

    private static string Format<TKey>(TKey value)
        where TKey : IFormattable =>
        value switch
        {
            DateTimeOffset moment => moment.ToString("O", CultureInfo.InvariantCulture),
            DateTime moment => moment.ToString("O", CultureInfo.InvariantCulture),
            _ => value.ToString(null, CultureInfo.InvariantCulture),
        };

    private static bool TryParse<TKey>(string text, out TKey value)
        where TKey : struct
    {
        value = default;
        if (typeof(TKey) == typeof(DateTimeOffset))
        {
            var ok = DateTimeOffset.TryParseExact(text, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var moment);
            value = ok ? (TKey)(object)moment : default;
            return ok;
        }

        if (typeof(TKey) == typeof(DateTime))
        {
            var ok = DateTime.TryParseExact(text, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var moment);
            value = ok ? (TKey)(object)moment : default;
            return ok;
        }

        if (typeof(TKey) == typeof(long))
        {
            var ok = long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number);
            value = ok ? (TKey)(object)number : default;
            return ok;
        }

        if (typeof(TKey) == typeof(int))
        {
            var ok = int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number);
            value = ok ? (TKey)(object)number : default;
            return ok;
        }

        throw new NotSupportedException($"游标分页不支持 {typeof(TKey).Name} 作为排序键，请用 DateTimeOffset、DateTime、int 或 long。");
    }

    // 把选择器里的参数换成同一个参数，两个选择器才能合进一个谓词，EF 才翻译得了
    private sealed class ParameterReplacer(ParameterExpression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => replacement;
    }

    // 强类型 ID 由 Vogen 生成，有静态的 From(Guid)；游标里存的是 Guid，翻页时要变回强类型 ID 才能和列比较
    private static class StronglyTypedIds<TId>
        where TId : struct, IStronglyTypedId
    {
        public static readonly Func<Guid, TId> FromGuid = typeof(TId).GetMethod("From", BindingFlags.Public | BindingFlags.Static, [typeof(Guid)])
            ?.CreateDelegate<Func<Guid, TId>>()
            ?? throw new InvalidOperationException($"{typeof(TId).Name} 没有 From(Guid) 静态方法，游标分页要求强类型 ID 由 Vogen 生成。");
    }
}
