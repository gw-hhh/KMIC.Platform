namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 操作结果
/// </summary>
/// <remarks>
/// 用类而不是结构体，default 才不会被当成成功
/// </remarks>
public class Result : IResultFactory<Result>
{
    private static readonly Result _success = new(null);

    private readonly Error? _error;

    /// <summary>
    /// error 为空表示成功
    /// </summary>
    protected Result(Error? error) => _error = error;

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => _error is null;

    /// <summary>
    /// 是否失败
    /// </summary>
    public bool IsFailure => _error is not null;

    /// <summary>
    /// 错误，成功时访问会抛异常
    /// </summary>
    public Error Error => _error ?? throw new InvalidOperationException("成功的结果没有 Error。");

    /// <summary>
    /// 成功
    /// </summary>
    public static Result Success() => _success;

    /// <summary>
    /// 失败
    /// </summary>
    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(error);
    }

    /// <summary>
    /// 返回第一个失败，全部成功时返回成功
    /// </summary>
    public static Result FirstFailure(params ReadOnlySpan<Result> results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return result;
            }
        }

        return _success;
    }

    /// <summary>
    /// 错误可以直接作为失败结果返回
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    static Result IResultFactory<Result>.Fail(Error error) => Failure(error);

    /// <summary>
    /// 按成功或失败分别处理
    /// </summary>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return _error is null ? onSuccess() : onFailure(_error);
    }
}
