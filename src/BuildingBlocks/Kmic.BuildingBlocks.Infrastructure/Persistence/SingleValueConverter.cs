using System.Linq.Expressions;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 单值值对象转换器
/// </summary>
/// <remarks>
/// 读取时调用私有构造函数，不走工厂方法的校验：规则收紧后，旧数据仍然能读出来再修正
/// </remarks>
internal sealed class SingleValueConverter<TObject, TValue>() : ValueConverter<TObject, TValue>(_toProvider, _fromProvider)
    where TObject : class, ISingleValueObject<TValue>
{
    private static readonly Expression<Func<TObject, TValue>> _toProvider = valueObject => valueObject.Value;

    private static readonly Expression<Func<TValue, TObject>> _fromProvider = BuildFromProvider();

    private static Expression<Func<TValue, TObject>> BuildFromProvider()
    {
        var constructor = typeof(TObject).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, [typeof(TValue)])
            ?? throw new InvalidOperationException($"{typeof(TObject).Name} 缺少只接收 {typeof(TValue).Name} 的私有构造函数。");
        var value = Expression.Parameter(typeof(TValue), "value");
        return Expression.Lambda<Func<TValue, TObject>>(Expression.New(constructor, value), value);
    }
}
