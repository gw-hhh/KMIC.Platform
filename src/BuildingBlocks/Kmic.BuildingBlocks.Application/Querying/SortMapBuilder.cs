using System.Linq.Expressions;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>逐个注册可排序字段，并通过 <see cref="Default"/> 指定默认排序完成构建。</summary>
public sealed class SortMapBuilder<T>
{
    private readonly Dictionary<string, LambdaExpression> _fields = new(StringComparer.Ordinal);

    internal SortMapBuilder()
    {
    }

    public SortMapBuilder<T> Field<TKey>(string name, Expression<Func<T, TKey>> selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(selector);

        if (name.StartsWith('-') || name.Contains(',', StringComparison.Ordinal))
        {
            throw new DeclarationException($"排序字段名“{name}”不能以 - 开头或包含逗号。");
        }

        return _fields.TryAdd(name, selector)
            ? this
            : throw new DeclarationException($"{typeof(T).Name} 的排序字段“{name}”重复注册。");
    }

    public SortMap<T> Default(string sort)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sort);
        return new SortMap<T>(_fields, sort);
    }
}
