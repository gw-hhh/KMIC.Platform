using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 列表允许排序的字段
/// </summary>
/// <remarks>
/// 在列表的静态字段里声明一次，请求里的排序字段只能是这里登记的名字，不会拼进 SQL。
/// 框架在最后追加 Id 作为排序键，保证同值的记录在分页之间不漂移，所以列表项类型要有 Id 属性
/// </remarks>
/// <typeparam name="T">列表项类型，排序在投影之后的列表项上做</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "声明写法是 SortMap<列表项>.Field(...)，类型参数写在左边，调用处不用再写一遍")]
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

    /// <summary>
    /// 登记的字段名，按声明顺序，OpenAPI 文档里列出来给前端
    /// </summary>
    public IReadOnlyList<string> Fields { get; }

    /// <summary>
    /// 请求没有给排序时使用的排序
    /// </summary>
    public string DefaultSort { get; }

    /// <summary>
    /// 开始声明：登记第一个可排序的字段
    /// </summary>
    /// <param name="name">请求里使用的字段名，小驼峰</param>
    /// <param name="selector">列表项上的排序依据</param>
    public static SortMapBuilder<T> Field<TKey>(string name, Expression<Func<T, TKey>> selector) =>
        new SortMapBuilder<T>().Field(name, selector);

    /// <summary>
    /// 解析请求里的排序表达式，空表示默认排序
    /// </summary>
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
        // 最后按 Id 排，同值的记录在页与页之间的先后才固定
        SortKey[] keys = [.. terms.Select(t => new SortKey(_fields[t.Name], t.Descending)), new SortKey(_tieBreaker, Descending: false)];
        return ProjectionOrdering.Order(query, keys);
    }

    private static Result<SortTerm[]> Invalid(string message) =>
        CommonErrors.ValidationFailedWith([new FieldError("sort", "Invalid", message)]);

    private static LambdaExpression IdSelector()
    {
        var property = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new DeclarationException($"{typeof(T).Name} 没有 Id 属性。排序要在最后追加 Id，同值的记录才不会在分页之间漂移。");
        var parameter = Expression.Parameter(typeof(T), "x");
        return Expression.Lambda(Expression.Property(parameter, property), parameter);
    }
}
