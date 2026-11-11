using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// ProblemDetails 的唯一写出方
/// </summary>
/// <remarks>
/// 不看请求的 Accept：这是纯 JSON 接口，任何错误都用 application/problem+json 写出。
/// 业务错误已经带齐扩展字段；中间件只设了状态码的，这里按状态码补成统一格式
/// </remarks>
internal sealed class KmicProblemDetailsWriter(IOptions<HttpJsonOptions> json) : IProblemDetailsWriter
{
    private const string ContentType = "application/problem+json";

    public bool CanWrite(ProblemDetailsContext context) => true;

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var http = context.HttpContext;
        var details = context.ProblemDetails;
        details.Status ??= http.Response.StatusCode;
        if (!details.Extensions.ContainsKey(KmicProblems.CodeKey))
        {
            details = KmicProblems.From(FrameworkProblems.ForStatus(details.Status.Value), http);
        }

        http.Response.StatusCode = details.Status ?? http.Response.StatusCode;
        return new ValueTask(http.Response.WriteAsJsonAsync(details, json.Value.SerializerOptions, ContentType, http.RequestAborted));
    }
}
