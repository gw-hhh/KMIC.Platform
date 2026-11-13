using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Errors;
using Microsoft.Net.Http.Headers;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 把一个 <see cref="FileDownload" /> 写成响应：先打开对象，打不开按错误响应，打开了才写头和内容
/// </summary>
/// <remarks>
/// 响应头：Content-Disposition 带 ASCII 回退名与 UTF-8 编码的原名（RFC 6266）；Content-Type 用识别出的类型；
/// nosniff；Cache-Control 为 private, no-store。直接在浏览器里打开时加上沙箱式的 CSP，防止上传的文件在本站点下执行脚本
/// </remarks>
internal sealed class FileDownloadResult(FileDownload file, bool inline) : IResult
{
    private const string InlinePolicy = "sandbox; default-src 'none'; img-src 'self'; style-src 'unsafe-inline'";

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        Stream content;
        try
        {
            content = await httpContext.RequestServices.GetRequiredService<IFileContentReader>().OpenReadAsync(file.StorageKey, httpContext.RequestAborted);
        }
        catch (FileNotFoundException)
        {
            // 记录在、对象不在：对象被删除标记覆盖或丢失，等运维还原；不能让人误以为是权限问题
            await new ProblemResult(CommonErrors.FileContentMissing).ExecuteAsync(httpContext);
            return;
        }

        var headers = httpContext.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.CacheControl = "private, no-store";
        headers.ContentDisposition = DispositionOf(file.Name, inline);
        if (inline)
        {
            headers.ContentSecurityPolicy = InlinePolicy;
        }

        var etag = new EntityTagHeaderValue($"\"{file.Sha256}\"");
        await Results.Stream(content, file.ContentType, fileDownloadName: null, lastModified: null, entityTag: etag, enableRangeProcessing: true).ExecuteAsync(httpContext);
    }

    private static string DispositionOf(string name, bool inline)
    {
        var disposition = new ContentDispositionHeaderValue(inline ? "inline" : "attachment");
        disposition.SetHttpFileName(name);
        return disposition.ToString();
    }
}
