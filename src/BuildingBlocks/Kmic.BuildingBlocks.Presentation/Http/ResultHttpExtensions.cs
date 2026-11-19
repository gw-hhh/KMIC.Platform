using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Net.Http.Headers;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// Result 转 HTTP 响应
/// </summary>
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
    /// 成功返回 200 和值并带 ETag，If-None-Match 一致时返回 304
    /// </summary>
    /// <remarks>
    /// 给变化少、反复读的数据用，如字典
    /// </remarks>
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
    public static IResult ToAccepted(this Result<JobAccepted> result, Func<Guid, string> location)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);

        return result.IsSuccess ? TypedResults.Accepted(location(result.Value.JobId), result.Value) : Problem(result);
    }

    private static ProblemResult Problem(Result failure) => new(failure.Error);
}
