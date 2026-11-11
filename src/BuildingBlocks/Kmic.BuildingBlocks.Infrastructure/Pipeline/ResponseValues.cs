using System.Linq.Expressions;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 读取和构造结果里的值
/// </summary>
/// <remarks>
/// 管道不知道具体的值类型，幂等重放和版本号回填要用它
/// </remarks>
internal static class ResponseValues<TResponse>
    where TResponse : Result
{
    /// <summary>
    /// 值类型，Result 没有值时为空
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

    // 放在嵌套类里，等外层的 ValueType 初始化后，第一次用到时才编译
    private static class Compiled
    {
        public static readonly Func<TResponse, object?> Read = BuildRead();

        public static readonly Func<object?, TResponse> Create = BuildCreate();
    }
}
