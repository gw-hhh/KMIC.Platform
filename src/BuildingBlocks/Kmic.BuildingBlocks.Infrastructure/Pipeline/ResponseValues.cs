using System.Linq.Expressions;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 在不知道具体值类型的管道中读取、构造 Result 与 Result&lt;T&gt; 的值：幂等重放、版本回填要用
/// </summary>
internal static class ResponseValues<TResponse>
    where TResponse : Result
{
    /// <summary>
    /// 成功时的值类型；Result 没有值，为空
    /// </summary>
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

    // Result<T> 由 T 的隐式转换构造
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

    // 放在嵌套类中：等外层的 ValueType 初始化之后，第一次使用时才编译
    private static class Compiled
    {
        public static readonly Func<TResponse, object?> Read = BuildRead();

        public static readonly Func<object?, TResponse> Create = BuildCreate();
    }
}
