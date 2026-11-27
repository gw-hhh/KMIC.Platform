using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Net.Http.Headers;

namespace Kmic.BuildingBlocks.Presentation.Http;

public static class ResultHttpExtensions
{
    /// <summary>成功时返回 200 及响应体</summary>
    public static IResult ToOk<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result);
    }

    /// <summary>成功时返回 200 及 ETag（命中 If-None-Match 时返回 304），适合低频变更的高频查询</summary>
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

    /// <summary>成功时返回 204</summary>
    public static IResult ToNoContent(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.NoContent() : Problem(result);
    }

    /// <summary>成功时返回 201、Location 头及新资源 ID 与版本号</summary>
    public static IResult ToCreated<TId>(this Result<CreatedResponse<TId>> result, Func<TId, string> location)
        where TId : struct, IStronglyTypedId
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);

        return result.IsSuccess ? TypedResults.Created(location(result.Value.Id), result.Value) : Problem(result);
    }

    /// <summary>成功时返回 202、Location 头及后台作业信息</summary>
    public static IResult ToAccepted(this Result<JobAccepted> result, Func<Guid, string> location)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);

        return result.IsSuccess ? TypedResults.Accepted(location(result.Value.JobId), result.Value) : Problem(result);
    }

    private static ProblemResult Problem(Result failure) => new(failure.Error);
}
