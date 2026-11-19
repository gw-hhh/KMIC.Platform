namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 文件存储
/// </summary>
/// <remarks>
/// 只有 Files 模块和框架基础设施能用
/// </remarks>
public interface IFileStorage
{
    /// <summary>
    /// 流式写入，大文件在内部分段上传
    /// </summary>
    Task<StoredObject> WriteAsync(string key, Stream content, string contentType, CancellationToken ct);

    /// <summary>
    /// 打开读取，对象不存在时抛 FileNotFoundException
    /// </summary>
    Task<Stream> OpenReadAsync(string key, CancellationToken ct);

    /// <summary>
    /// 删除，对象不存在时不报错
    /// </summary>
    /// <remarks>
    /// 开启版本控制的桶里只产生删除标记
    /// </remarks>
    Task DeleteAsync(string key, CancellationToken ct);

    /// <summary>
    /// 对象是否存在
    /// </summary>
    /// <remarks>
    /// 给了版本号看那个版本，不给时被删除标记覆盖的算不存在
    /// </remarks>
    Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct);

    /// <summary>
    /// 带有效期的下载地址，只有 S3 实现支持，本地存储抛 NotSupportedException，调用方退回由应用转发
    /// </summary>
    Task<Uri> GetDownloadUrlAsync(string key, TimeSpan validFor, string downloadName, CancellationToken ct);

    /// <summary>
    /// 列出前缀下的全部存储键，对账用
    /// </summary>
    IAsyncEnumerable<string> ListAsync(string prefix, CancellationToken ct);
}
