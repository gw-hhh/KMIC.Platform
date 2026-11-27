namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>文件存储接口（仅限 Files 模块及框架基础设施内部使用）</summary>
public interface IFileStorage
{
    /// <summary>流式写入，大文件在内部自动分段上传</summary>
    Task<StoredObject> WriteAsync(string key, Stream content, string contentType, CancellationToken ct);

    /// <summary>打开读取流，对象不存在时抛出 FileNotFoundException</summary>
    Task<Stream> OpenReadAsync(string key, CancellationToken ct);

    /// <summary>删除对象（不存在时静默忽略；开启版本控制的桶仅写入删除标记）</summary>
    Task DeleteAsync(string key, CancellationToken ct);

    /// <summary>判断对象是否存在</summary>
    Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct);

    /// <summary>生成带有效期的直链下载地址（仅 S3 存储支持）</summary>
    Task<Uri> GetDownloadUrlAsync(string key, TimeSpan validFor, string downloadName, CancellationToken ct);

    /// <summary>列举指定前缀下的全部存储键（用于存储对账）</summary>
    IAsyncEnumerable<string> ListAsync(string prefix, CancellationToken ct);
}
