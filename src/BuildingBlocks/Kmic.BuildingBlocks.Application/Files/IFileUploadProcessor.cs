namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>上传文件流处理器；不持有数据库事务，调用方须在前后分别通过短事务维护文件状态。</summary>
public interface IFileUploadProcessor
{
    /// <summary>处理并写入上传文件流；客户端断开、超时、取消或存储写入失败时抛出异常。</summary>
    Task<UploadResult> StoreAsync(UploadProfile profile, string fileName, Stream content, string key, CancellationToken ct);
}
