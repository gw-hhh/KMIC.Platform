namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 文件存储
/// </summary>
/// <remarks>
/// 只有 Files 模块和框架基础设施能用。存储键和原始文件名无关，由文件 ID 生成，从不覆盖
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
    /// 开启版本控制的桶里只产生删除标记，旧版本保留到生命周期规则到期
    /// </remarks>
    Task DeleteAsync(string key, CancellationToken ct);

    /// <summary>
    /// 对象是否存在
    /// </summary>
    /// <remarks>
    /// 不给版本号时看当前对象，被删除标记覆盖的算不存在；给了版本号就看那个版本是否还在，
    /// 对账用它区分“只是被删除标记覆盖、可以还原”和“版本也没了”。本地存储没有版本，忽略版本号
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
