using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Net.Http.Headers;

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
    /// 成功返回 200 和值，并带上 ETag；请求的 If-None-Match 与它一致时返回 304、没有响应体
    /// </summary>
    /// <remarks>
    /// 给“变化很少、很多人反复读”的数据用，如字典：前端带着上次的 ETag 再问，没变就不用再传一遍。
    /// 响应同时带 <c>Cache-Control: private, no-cache</c>：浏览器可以保存，但每次使用前必须带 ETag 向服务器确认
    /// </remarks>
    /// <param name="result">查询的结果</param>
    /// <param name="http">当前请求</param>
    /// <param name="etagOf">从值算出 ETag 的内容，不用带引号；值不变它就不变</param>
    public static IResult ToOkWithETag<T>(this Result<T> result, HttpContext http, Func<T, string> etagOf)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(etagOf);

        if (result.IsFailure)
        {
            return Problem(result);
        }

        var etag = new EntityTagHeaderValue($"\"{etagOf(result.Value)}\"");
        var response = http.Response.GetTypedHeaders();
        response.ETag = etag;
        response.CacheControl = new CacheControlHeaderValue { Private = true, NoCache = true };
        var presented = http.Request.GetTypedHeaders().IfNoneMatch;
        return presented.Any(t => t.Equals(EntityTagHeaderValue.Any) || t.Compare(etag, useStrongComparison: false))
            ? Results.StatusCode(StatusCodes.Status304NotModified)
            : TypedResults.Ok(result.Value);
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
