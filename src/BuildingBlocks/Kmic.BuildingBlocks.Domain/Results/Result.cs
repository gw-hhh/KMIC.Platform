namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>操作结果。</summary>
public class Result : IResultFactory<Result>
{
    private static readonly Result _success = new(null);

    private readonly Error? _error;

    /// <summary>初始化操作结果；<paramref name="error" /> 为 null 表示成功。</summary>
    protected Result(Error? error) => _error = error;

    public bool IsSuccess => _error is null;

    public bool IsFailure => _error is not null;

    /// <summary>失败时的错误信息；结果成功时访问将抛出异常。</summary>
    public Error Error => _error ?? throw new InvalidOperationException("成功的结果没有 Error。");

    public static Result Success() => _success;

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(error);
    }

    /// <summary>返回首个失败结果；全部成功时返回成功。</summary>
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

    public static implicit operator Result(Error error) => Failure(error);

    static Result IResultFactory<Result>.Fail(Error error) => Failure(error);

    /// <summary>根据成功或失败状态分别执行对应分支。</summary>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return _error is null ? onSuccess() : onFailure(_error);
    }
}
