using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>列表允许排序的字段映射（基于投影后的列表项定义，且列表项类型须包含 Id 属性）。</summary>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "采用 SortMap<TItem>.Field(...) 声明语法，由外层泛型类固定列表项类型，避免调用处重复指定类型参数")]
public sealed class SortMap<T>
{
    private readonly FrozenDictionary<string, LambdaExpression> _fields;
    private readonly LambdaExpression _tieBreaker;
    private readonly SortTerm[] _default;

    internal SortMap(IReadOnlyDictionary<string, LambdaExpression> fields, string defaultSort)
    {
        _fields = fields.ToFrozenDictionary(StringComparer.Ordinal);
        _tieBreaker = IdSelector();
        Fields = [.. fields.Keys];
        DefaultSort = defaultSort;
        _default = Parse(defaultSort).Match(
            terms => terms,
            error => throw new DeclarationException($"{typeof(T).Name} 的默认排序“{defaultSort}”不合法：{error.FieldErrors[0].Message}"));
    }

    /// <summary>可排序字段名列表，按声明顺序排列。</summary>
    public IReadOnlyList<string> Fields { get; }

    public string DefaultSort { get; }

    /// <summary>开始构建排序映射并注册首个可排序字段。</summary>
    public static SortMapBuilder<T> Field<TKey>(string name, Expression<Func<T, TKey>> selector) =>
        new SortMapBuilder<T>().Field(name, selector);

    internal Result<SortTerm[]> Parse(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return _default;
        }

        var parts = sort.Split(',');
        if (parts.Length > SortRequest.MaxFields)
        {
            return Invalid($"最多按 {SortRequest.MaxFields} 个字段排序");
        }

        var terms = new List<SortTerm>(parts.Length);
        foreach (var part in parts)
        {
            var descending = part.StartsWith('-');
            var name = descending ? part[1..] : part;
            if (!_fields.ContainsKey(name))
            {
                return Invalid($"不支持按“{name}”排序，可用字段：{string.Join("、", Fields)}");
            }

            if (terms.Exists(t => string.Equals(t.Name, name, StringComparison.Ordinal)))
            {
                return Invalid($"排序字段“{name}”重复");
            }

            terms.Add(new SortTerm(name, descending));
        }

        return terms.ToArray();
    }

    internal IQueryable<T> Apply(IQueryable<T> query, IReadOnlyList<SortTerm> terms)
    {
        // 末尾追加 Id 排序，确保排序值相同的记录在分页间顺序稳定
        SortKey[] keys = [.. terms.Select(t => new SortKey(_fields[t.Name], t.Descending)), new SortKey(_tieBreaker, Descending: false)];
        return ProjectionOrdering.Order(query, keys);
    }

    private static Result<SortTerm[]> Invalid(string message) =>
        CommonErrors.ValidationFailedWith([new FieldError("sort", "Invalid", message)]);

    private static LambdaExpression IdSelector()
    {
        var property = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new DeclarationException($"{typeof(T).Name} 缺少 Id 属性。排序末尾须追加 Id 作为决胜键，以防排序值相同时分页顺序漂移。");
        var parameter = Expression.Parameter(typeof(T), "x");
        return Expression.Lambda(Expression.Property(parameter, property), parameter);
    }
}
