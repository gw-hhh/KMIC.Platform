using Kmic.BuildingBlocks.Application.Files;

namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 处理一个上传的文件流：识别类型、检查上限、边读边算 SHA-256、流式写入存储
/// </summary>
/// <remarks>
/// 不整体读入内存，也不落本地临时文件。写存储期间不持有数据库事务，数据库的登记由调用方在前后各用一个短事务完成。
/// 类型不符、空文件在写入之前就拒绝，存储里不留对象；读流时发现超过上限，中断写入，存储自己清理写了一半的内容
/// （本地存储删临时文件，S3 中止分段上传）。其他异常（客户端断开、超时、取消）照常向上抛，由调用方把这次上传登记为被拒绝
/// </remarks>
public sealed class FileUploadProcessor(IFileStorage storage)
{
    /// <summary>
    /// 处理上传的文件流
    /// </summary>
    /// <param name="profile">上传档案：大小上限与允许的类型</param>
    /// <param name="fileName">客户端给的文件名，只用它的扩展名识别类型</param>
    /// <param name="content">文件内容，读到结尾为止</param>
    /// <param name="key">写入存储用的键，由文件 ID 生成，与文件名无关</param>
    /// <param name="ct">取消令牌</param>
    public async Task<UploadResult> StoreAsync(UploadProfile profile, string fileName, Stream content, string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

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

        return new UploadResult(UploadOutcome.Stored, new StoredFileInfo(stored, source.BytesRead, source.FinishHash(), type));
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
