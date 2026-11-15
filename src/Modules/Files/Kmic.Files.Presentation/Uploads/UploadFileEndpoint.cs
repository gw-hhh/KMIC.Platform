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
/// 以 multipart/form-data 流式上传单个文件，不将完整内容读入内存
/// </summary>
/// <remarks>
/// 先用 StartUpload 登记上传，再流式写入存储，最后用 CompleteUpload 或 RejectUpload 登记结果。
/// 两次登记各用短事务，写存储期间不持有数据库事务；写入时检查类型和大小，失败后的残留对象由清理与对账处理
/// </remarks>
internal sealed class UploadFileEndpoint : IEndpoint
{
    // 请求体上限预留 multipart 边界和段头所需空间
    private const long BodyOverhead = 64 * 1024;

    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/", HandleAsync)
            .Sends<StartUploadCommand>()
            .Accepts<IFormFile>("multipart/form-data")
            .WithRequestTimeout(TimeSpan.FromMinutes(10))
            .DisableAntiforgery()
            .WithSummary("上传文件")
            .WithDescription("使用 multipart/form-data，仅处理第一个带文件名的部分。profile 指定上传配置，决定大小上限、允许类型和所需权限；按扩展名及文件头识别类型，不采用客户端声明的内容类型。成功后返回临时文件 ID，须在保留期内（默认 24 小时）关联业务对象，未关联的到期后可被清理。超过大小上限返回 413，类型不允许返回 415。");

    private static async Task<IResult> HandleAsync(
        HttpContext http,
        [Description("上传配置名称，如 Sample.SpecimenAttachment")] string profile,
        [Description("文件大小（字节），可选；声明大小超过配置上限时直接拒绝，实际读取仍受该上限限制")] long? size,
        ICommandSender sender,
        UploadProfileCatalog profiles,
        IFileUploadProcessor processor,
        CancellationToken ct)
    {
        if (profiles.Find(profile) is not { } declared)
        {
            return FailureOf(FilesErrors.ProfileNotFound);
        }

        // 请求体按所有上传配置中的最大上限预留空间，文件流仍受当前配置限制
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

    // 优先使用 RFC 5987 的 filename*，兼容带编码的文件名
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
