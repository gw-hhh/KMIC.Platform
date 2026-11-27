namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>带返回值的操作结果。</summary>
public sealed class Result<T> : Result, IResultFactory<Result<T>>
{
    private readonly T _value;

    private Result(T value)
        : base(null) => _value = value;

    // 失败状态下外部无法读取 _value，访问 Value 属性会先抛出异常
    private Result(Error error)
        : base(error) => _value = default!;

    /// <summary>成功时的返回值；结果失败时访问将抛出异常。</summary>
    public T Value => IsSuccess ? _value : throw new InvalidOperationException($"失败的结果没有值（{Error.Code}）。");

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(error);
    }

    // 显式实现接口静态成员，规避泛型类型上的 CA1000 警告
    static Result<T> IResultFactory<Result<T>>.Fail(Error error) => error;

    /// <summary>成功时映射返回值，失败时透传原错误。</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return IsSuccess ? map(_value) : Error;
    }

    /// <summary>成功时串联执行后续操作，失败时透传原错误。</summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return IsSuccess ? bind(_value) : Error;
    }

    /// <summary>根据成功或失败状态分别执行对应分支。</summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(_value) : onFailure(Error);
    }
}
