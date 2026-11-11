using System.Linq.Expressions;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 逐个登记可排序的字段，最后用 <see cref="Default"/> 给出默认排序并完成
/// </summary>
/// <typeparam name="T">列表项类型</typeparam>
public sealed class SortMapBuilder<T>
{
    private readonly Dictionary<string, LambdaExpression> _fields = new(StringComparer.Ordinal);

    internal SortMapBuilder()
    {
    }

    /// <summary>
    /// 登记一个可排序的字段
    /// </summary>
    /// <param name="name">请求里使用的字段名，小驼峰</param>
    /// <param name="selector">列表项上的排序依据</param>
    public SortMapBuilder<T> Field<TKey>(string name, Expression<Func<T, TKey>> selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(selector);

        if (name.StartsWith('-') || name.Contains(',', StringComparison.Ordinal))
        {
            throw new DeclarationException($"排序字段名“{name}”不能以 - 开头，也不能含逗号。");
        }

        return _fields.TryAdd(name, selector)
            ? this
            : throw new DeclarationException($"{typeof(T).Name} 的排序字段“{name}”登记了两次。");
    }

    /// <summary>
    /// 给出默认排序并完成登记
    /// </summary>
    /// <param name="sort">写法与请求相同，如 -createdAt</param>
    public SortMap<T> Default(string sort)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sort);
        return new SortMap<T>(_fields, sort);
    }
}
