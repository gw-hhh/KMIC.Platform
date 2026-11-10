namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 带返回值的操作结果：成功时有 Value，失败时有 Error
/// </summary>
public sealed class Result<T> : Result, IResultFactory<Result<T>>
{
    private readonly T _value;

    private Result(T value)
        : base(null) => _value = value;

    private Result(Error error)
        : base(error) => _value = default!;

    /// <summary>
    /// 成功时的值；在失败的结果上访问属于程序缺陷，会抛出异常
    /// </summary>
    public T Value => IsSuccess ? _value : throw new InvalidOperationException($"失败的结果没有值（{Error.Code}）。");

    /// <summary>
    /// 让方法可以直接 return 一个值
    /// </summary>
    public static implicit operator Result<T>(T value) => new(value);

    /// <summary>
    /// 让错误目录中的 Error 可以直接作为失败结果返回
    /// </summary>
    public static implicit operator Result<T>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(error);
    }

    // 显式接口实现：不是公开的静态成员，泛型类型上也不触发 CA1000
    static Result<T> IResultFactory<Result<T>>.Fail(Error error) => error;

    /// <summary>
    /// 成功时转换值，失败时原样传递错误
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return IsSuccess ? map(_value) : Error;
    }

    /// <summary>
    /// 成功时继续执行下一步可能失败的操作，失败时原样传递错误
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return IsSuccess ? bind(_value) : Error;
    }

    /// <summary>
    /// 两个分支归一，主要给接口层把结果转成响应
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(_value) : onFailure(Error);
    }
}
