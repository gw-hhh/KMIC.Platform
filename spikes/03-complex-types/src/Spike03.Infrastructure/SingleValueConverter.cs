using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Spike03.Domain;

namespace Spike03.Infrastructure;

/// <summary>
/// 单值值对象与原始值之间的转换；读库时调用私有构造函数，不经过工厂方法的校验（05-01 第 5.3 节）
/// </summary>
public sealed class SingleValueConverter<TObject, TValue>() : ValueConverter<TObject, TValue>(_toProvider, _fromProvider)
    where TObject : class, ISingleValueObject<TValue>
{
    private static readonly Expression<Func<TObject, TValue>> _toProvider = o => o.Value;

    private static readonly Expression<Func<TValue, TObject>> _fromProvider = BuildFromProvider();

    private static Expression<Func<TValue, TObject>> BuildFromProvider()
    {
        var constructor = typeof(TObject).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, [typeof(TValue)])
            ?? throw new InvalidOperationException($"{typeof(TObject).Name} needs a private constructor taking {typeof(TValue).Name}.");
        var value = Expression.Parameter(typeof(TValue), "value");
        return Expression.Lambda<Func<TValue, TObject>>(Expression.New(constructor, value), value);
    }
}
