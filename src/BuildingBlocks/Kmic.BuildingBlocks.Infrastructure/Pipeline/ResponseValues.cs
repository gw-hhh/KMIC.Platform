using System.Linq.Expressions;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

internal static class ResponseValues<TResponse>
    where TResponse : Result
{
    // 非泛型 Result 时为 null
    public static Type? ValueType { get; } = typeof(TResponse).IsGenericType ? typeof(TResponse).GetGenericArguments()[0] : null;

    public static object? Read(TResponse response) => Compiled.Read(response);

    public static TResponse Create(object? value) => Compiled.Create(value);

    private static Func<TResponse, object?> BuildRead()
    {
        if (ValueType is null)
        {
            return _ => null;
        }

        var response = Expression.Parameter(typeof(TResponse), "response");
        var value = Expression.Convert(Expression.Property(response, nameof(Result<>.Value)), typeof(object));
        return Expression.Lambda<Func<TResponse, object?>>(value, response).Compile();
    }

    // Result<T> 通过 T 的隐式转换构造
    private static Func<object?, TResponse> BuildCreate()
    {
        if (ValueType is null)
        {
            return _ => (TResponse)Result.Success();
        }

        var value = Expression.Parameter(typeof(object), "value");
        var response = Expression.Convert(Expression.Convert(value, ValueType), typeof(TResponse));
        return Expression.Lambda<Func<object?, TResponse>>(response, value).Compile();
    }

    // 置于嵌套类中，确保外层 ValueType 初始化完成并在首次使用时才编译委托
    private static class Compiled
    {
        public static readonly Func<TResponse, object?> Read = BuildRead();

        public static readonly Func<object?, TResponse> Create = BuildCreate();
    }
}
