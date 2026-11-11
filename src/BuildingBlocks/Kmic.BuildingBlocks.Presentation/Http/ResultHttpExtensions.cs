using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// Result 转 HTTP 响应
/// </summary>
/// <remarks>
/// 端点里只用这几个方法把 Result 变成响应，不手写 Results.Problem 或 Results.StatusCode：
/// 错误只能来自 Result，状态码和格式才统一
/// </remarks>
public static class ResultHttpExtensions
{
    /// <summary>
    /// 成功返回 200 和值
    /// </summary>
    public static IResult ToOk<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result);
    }

    /// <summary>
    /// 成功返回 204，没有响应体
    /// </summary>
    public static IResult ToNoContent(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.NoContent() : Problem(result);
    }

    /// <summary>
    /// 成功返回 201，Location 指向新数据，响应体是新数据的 ID 和版本号
    /// </summary>
    /// <param name="result">创建命令的结果</param>
    /// <param name="location">用新 ID 拼出新数据的地址</param>
    public static IResult ToCreated<TId>(this Result<CreatedResponse<TId>> result, Func<TId, string> location)
        where TId : struct, IStronglyTypedId
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);

        return result.IsSuccess ? TypedResults.Created(location(result.Value.Id), result.Value) : Problem(result);
    }

    /// <summary>
    /// 成功返回 202，Location 指向后台作业，响应体是作业 ID
    /// </summary>
    /// <param name="result">提交后台作业的命令的结果</param>
    /// <param name="location">用作业 ID 拼出作业资源的地址</param>
    public static IResult ToAccepted(this Result<JobAccepted> result, Func<Guid, string> location)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);

        return result.IsSuccess ? TypedResults.Accepted(location(result.Value.JobId), result.Value) : Problem(result);
    }

    private static ProblemResult Problem(Result failure) => new(failure.Error);
}
