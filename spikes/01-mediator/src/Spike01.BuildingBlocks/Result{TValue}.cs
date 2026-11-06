namespace Spike01.BuildingBlocks;

/// <summary>
/// 带值的结果：由值或 Error 隐式转换得到（05-01 第 2.3 节）
/// </summary>
public sealed class Result<TValue> : Result, IResultFactory<Result<TValue>>
{
    private readonly TValue? _value;

    private Result(TValue? value, Error? error)
        : base(error) => _value = value;

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Failed result has no value.");

    public static implicit operator Result<TValue>(TValue value) => new(value, null);

    public static implicit operator Result<TValue>(Error error) => new(default, error);

    // 显式接口实现：不是公开的静态成员，不触发 CA1000
    static Result<TValue> IResultFactory<Result<TValue>>.Fail(Error error) => new(default, error);
}
