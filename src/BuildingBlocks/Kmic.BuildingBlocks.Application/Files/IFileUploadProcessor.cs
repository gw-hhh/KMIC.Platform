namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 处理上传的文件流
/// </summary>
/// <remarks>
/// 不持有数据库事务，调用方前后各用短事务登记文件状态
/// </remarks>
public interface IFileUploadProcessor
{
    /// <summary>
    /// 处理上传的文件流
    /// </summary>
    /// <returns>写入成功，或没有写入的原因；客户端断开、超时、取消、存储写入失败向上抛异常</returns>
    Task<UploadResult> StoreAsync(UploadProfile profile, string fileName, Stream content, string key, CancellationToken ct);
}
