using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Application.Observability;

namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 文件上传处理
/// </summary>
public sealed class FileUploadProcessor(IFileStorage storage, FrameworkMetrics metrics) : IFileUploadProcessor
{
    /// <inheritdoc />
    public async Task<UploadResult> StoreAsync(UploadProfile profile, string fileName, Stream content, string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        UploadResult result;
        try
        {
            result = await ProcessAsync(profile, fileName, content, key, ct);
        }
        catch
        {
            metrics.FileUploadFailed(profile.Name);
            throw;
        }

        metrics.FileUploaded(profile.Name, result.Outcome, result.File?.Size ?? 0);
        return result;
    }

    private async Task<UploadResult> ProcessAsync(UploadProfile profile, string fileName, Stream content, string key, CancellationToken ct)
    {
        var header = await ReadHeaderAsync(content, ct);
        if (header.Length == 0)
        {
            return UploadResult.Refused(UploadOutcome.Empty);
        }

        var type = FileTypes.Identify(fileName, header);
        if (type is null || !profile.Allows(type.Code))
        {
            return UploadResult.Refused(UploadOutcome.TypeNotAllowed);
        }

        await using var source = new UploadSourceStream(header, content, profile.MaxBytes);
        StoredObject stored;
        try
        {
            stored = await storage.WriteAsync(key, source, type.ContentType, ct);
        }
        catch (UploadTooLargeException)
        {
            return UploadResult.Refused(UploadOutcome.TooLarge);
        }

        return new UploadResult(UploadOutcome.Stored, new StoredFileInfo(stored.Key, stored.VersionId, source.BytesRead, source.FinishHash(), type));
    }

    private static async Task<byte[]> ReadHeaderAsync(Stream content, CancellationToken ct)
    {
        var buffer = new byte[FileTypes.HeaderLength];
        var filled = 0;
        while (filled < buffer.Length)
        {
            var read = await content.ReadAsync(buffer.AsMemory(filled), ct);
            if (read == 0)
            {
                break;
            }

            filled += read;
        }

        return filled == buffer.Length ? buffer : buffer[..filled];
    }
}
