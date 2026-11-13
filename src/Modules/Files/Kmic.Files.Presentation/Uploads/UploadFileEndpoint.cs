using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Files.Application.Uploads.Complete;
using Kmic.Files.Application.Uploads.Reject;
using Kmic.Files.Application.Uploads.Start;
using Kmic.Files.Domain.Errors;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Kmic.Files.Presentation.Uploads;

/// <summary>
/// 上传端点：multipart/form-data，一次一个文件，流式读取、边读边写存储，不整体读入内存
/// </summary>
/// <remarks>
/// 三步：用 StartUpload 登记一行“上传中”（短事务）；文件流写入存储（不持有数据库事务）；
/// 用 CompleteUpload 或 RejectUpload 登记结果（短事务）。类型和大小的检查在写入过程中完成，不合规时没有对象留在存储里
/// </remarks>
internal sealed class UploadFileEndpoint : IEndpoint
{
    // multipart 的边界、段头占不了多少，给请求体上限留一点余量
    private const long BodyOverhead = 64 * 1024;

    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/", HandleAsync)
            .Sends<StartUploadCommand>()
            .Accepts<IFormFile>("multipart/form-data")
            .WithRequestTimeout(TimeSpan.FromMinutes(10))
            .DisableAntiforgery()
            .WithSummary("上传一个文件")
            .WithDescription("multipart/form-data，一次一个文件，第一个带文件名的部分就是文件。profile 是上传档案名，决定大小上限、允许的类型和需要的权限点；类型按扩展名加文件头识别，不信任客户端声明的内容类型。成功得到文件 ID：这是临时文件，到期（默认 24 小时）还没有被业务对象关联就会清理。文件太大返回 413，类型不允许返回 415。");

    private static async Task<IResult> HandleAsync(
        HttpContext http,
        [Description("上传档案名，如 Sample.ContractScan")] string profile,
        [Description("文件大小（字节），客户端知道时给，超过档案上限直接拒绝；不给就边读边按上限截断")] long? size,
        ICommandSender sender,
        UploadProfileCatalog profiles,
        IFileUploadProcessor processor,
        CancellationToken ct)
    {
        if (profiles.Find(profile) is not { } declared)
        {
            return FailureOf(FilesErrors.ProfileNotFound);
        }

        // 端点级的请求体上限按所有档案里最大的放宽，真正的上限在读流时按这个档案截断
        if (http.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } limit)
        {
            limit.MaxRequestBodySize = profiles.LargestMaxBytes + BodyOverhead;
        }

        if (await ReadFirstFileAsync(http.Request, ct) is not { } file)
        {
            return FailureOf(FilesErrors.EmptyFile);
        }

        var started = await sender.SendAsync(new StartUploadCommand(declared.Name, file.FileName, size ?? 0), ct);
        if (started.IsFailure)
        {
            return started.ToOk();
        }

        var result = await StoreAsync(processor, sender, declared, started.Value, file.Body, ct);
        if (result.File is not { } stored)
        {
            return await RejectAsync(sender, started.Value.FileId, result.Outcome);
        }

        var completed = await sender.SendAsync(
            new CompleteUploadCommand(started.Value.FileId, stored.Type.Code, stored.Type.ContentType, stored.Size, stored.Sha256, stored.VersionId),
            ct);
        return completed.IsSuccess ? Results.Json(completed.Value, statusCode: StatusCodes.Status201Created) : completed.ToOk();
    }

    // 取请求里第一个带文件名的部分；没有 multipart 边界或没有文件时返回空
    private static async Task<FilePart?> ReadFirstFileAsync(HttpRequest request, CancellationToken ct)
    {
        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var contentType)
            || !contentType.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase)
            || HeaderUtilities.RemoveQuotes(contentType.Boundary).Value is not { Length: > 0 } boundary)
        {
            return null;
        }

        var reader = new MultipartReader(boundary, request.Body);
        while (await reader.ReadNextSectionAsync(ct) is { } section)
        {
            if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition)
                && NameOf(disposition) is { Length: > 0 } fileName)
            {
                return new FilePart(fileName, section.Body);
            }
        }

        return null;
    }

    // 浏览器直接发 filename，也有客户端发 RFC 5987 编码的 filename*：后者带编码，优先用它
    private static string? NameOf(ContentDispositionHeaderValue disposition) =>
        HeaderUtilities.RemoveQuotes(disposition.FileNameStar.HasValue ? disposition.FileNameStar : disposition.FileName).Value;

    private static async Task<UploadResult> StoreAsync(
        IFileUploadProcessor processor,
        ICommandSender sender,
        UploadProfile profile,
        UploadStarted started,
        Stream body,
        CancellationToken ct)
    {
        try
        {
            return await processor.StoreAsync(profile, started.FileName, body, started.StorageKey, ct);
        }
        catch
        {
            // 客户端断开、超时、存储写入失败：登记为被拒绝，已经写进存储的部分由清理与对账处理
            await sender.SendAsync(new RejectUploadCommand(started.FileId, "上传中断"), CancellationToken.None);
            throw;
        }
    }

    private static async Task<IResult> RejectAsync(ICommandSender sender, Guid fileId, UploadOutcome outcome)
    {
        var (reason, error) = outcome switch
        {
            UploadOutcome.TooLarge => ("超过大小上限", FilesErrors.TooLarge),
            UploadOutcome.Empty => ("文件是空的", FilesErrors.EmptyFile),
            _ => ("类型不符", FilesErrors.TypeNotAllowed),
        };
        await sender.SendAsync(new RejectUploadCommand(fileId, reason), CancellationToken.None);
        return FailureOf(error);
    }

    private static IResult FailureOf(Error error)
    {
        Result<UploadCompleted> failure = error;
        return failure.ToOk();
    }

    private sealed record FilePart(string FileName, Stream Body);
}
