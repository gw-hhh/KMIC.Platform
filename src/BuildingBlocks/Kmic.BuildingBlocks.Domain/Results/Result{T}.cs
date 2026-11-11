namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 带返回值的操作结果
/// </summary>
public sealed class Result<T> : Result, IResultFactory<Result<T>>
{
    private readonly T _value;

    private Result(T value)
        : base(null) => _value = value;

    // 失败时读不到 _value：Value 会先抛异常
    private Result(Error error)
        : base(error) => _value = default!;

    /// <summary>
    /// 返回值，失败时访问会抛异常
    /// </summary>
    public T Value => IsSuccess ? _value : throw new InvalidOperationException($"失败的结果没有值（{Error.Code}）。");

    /// <summary>
    /// 值可以直接作为成功结果返回
    /// </summary>
    public static implicit operator Result<T>(T value) => new(value);

    /// <summary>
    /// 错误可以直接作为失败结果返回
    /// </summary>
    public static implicit operator Result<T>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(error);
    }

    // 显式实现，避开泛型类型上静态成员的 CA1000
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
    /// 成功时接着执行下一步，失败时原样传递错误
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return IsSuccess ? bind(_value) : Error;
    }

    /// <summary>
    /// 按成功或失败分别处理
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(_value) : onFailure(Error);
    }
}
