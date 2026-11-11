using System.Linq.Expressions;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 单值值对象存为它包装的原始值
/// </summary>
/// <remarks>
/// 读库时调用值对象的私有构造函数，不经过工厂方法的校验：规则收紧后旧数据仍能读出来修正（05-01 第 5.3 节）
/// </remarks>
internal sealed class SingleValueConverter<TObject, TValue>() : ValueConverter<TObject, TValue>(_toProvider, _fromProvider)
    where TObject : class, ISingleValueObject<TValue>
{
    private static readonly Expression<Func<TObject, TValue>> _toProvider = valueObject => valueObject.Value;

    private static readonly Expression<Func<TValue, TObject>> _fromProvider = BuildFromProvider();

    private static Expression<Func<TValue, TObject>> BuildFromProvider()
    {
        var constructor = typeof(TObject).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, [typeof(TValue)])
            ?? throw new InvalidOperationException($"单值值对象 {typeof(TObject).Name} 需要一个只接收 {typeof(TValue).Name} 的私有构造函数，供读库时使用。");
        var value = Expression.Parameter(typeof(TValue), "value");
        return Expression.Lambda<Func<TValue, TObject>>(Expression.New(constructor, value), value);
    }
}
