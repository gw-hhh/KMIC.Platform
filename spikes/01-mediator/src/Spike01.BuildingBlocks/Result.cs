namespace Spike01.BuildingBlocks;

/// <summary>
/// 不带值的结果（写法同 05-01 第 2.3 节）
/// </summary>
public class Result : IResultFactory<Result>
{
    protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => new(error);

    static Result IResultFactory<Result>.Fail(Error error) => new(error);
}
