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
    /// <param name="key">存储键，见 FileStorageKeys</param>
    /// <param name="content">文件内容，不要求能回到开头</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="ct">取消令牌</param>
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
    /// 带有效期的下载地址，只有 S3 实现支持，本地存储抛 NotSupportedException，调用方退回由应用转发
    /// </summary>
    /// <param name="key">存储键</param>
    /// <param name="validFor">有效期</param>
    /// <param name="downloadName">浏览器保存时用的文件名</param>
    /// <param name="ct">取消令牌</param>
    Task<Uri> GetDownloadUrlAsync(string key, TimeSpan validFor, string downloadName, CancellationToken ct);

    /// <summary>
    /// 列出前缀下的全部存储键，对账用
    /// </summary>
    IAsyncEnumerable<string> ListAsync(string prefix, CancellationToken ct);
}
