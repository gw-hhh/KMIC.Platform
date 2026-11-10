namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 不带返回值的操作结果：成功，或带一个 Error 的失败
/// </summary>
/// <remarks>
/// 用类而不是结构体：default 不会被误当成“成功”，Result&lt;T&gt; 也能继承它，管道行为只需处理一种类型。
/// </remarks>
public class Result
{
    private static readonly Result _success = new(null);

    private readonly Error? _error;

    /// <summary>
    /// 供派生类使用；error 为空表示成功
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
    /// 失败的原因；在成功的结果上访问属于程序缺陷，会抛出异常
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
    /// 多个结果中取第一个失败；全部成功时返回成功
    /// </summary>
    /// <remarks>
    /// 用于同时创建多个值对象的场景，避免写一串 if
    /// </remarks>
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
    /// 让错误目录中的 Error 可以直接作为失败结果返回
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// 两个分支归一，主要给接口层把结果转成响应
    /// </summary>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return _error is null ? onSuccess() : onFailure(_error);
    }
}
