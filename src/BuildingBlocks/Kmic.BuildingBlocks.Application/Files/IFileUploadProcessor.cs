namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 处理一个上传的文件流：识别类型、检查上限、边读边算 SHA-256、流式写入对象存储
/// </summary>
/// <remarks>
/// 上传端点用它。不整体读入内存，也不落本地临时文件；写存储期间不持有数据库事务，数据库的登记由调用方在前后各用一个短事务完成
/// </remarks>
public interface IFileUploadProcessor
{
    /// <summary>
    /// 处理上传的文件流
    /// </summary>
    /// <param name="profile">上传档案：大小上限与允许的类型</param>
    /// <param name="fileName">客户端给的文件名，只用它的扩展名识别类型</param>
    /// <param name="content">文件内容，读到结尾为止</param>
    /// <param name="key">写入存储用的键，由文件 ID 生成，与文件名无关</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>写入成功，或没有写入的原因；客户端断开、超时、取消、存储写入失败向上抛异常</returns>
    Task<UploadResult> StoreAsync(UploadProfile profile, string fileName, Stream content, string key, CancellationToken ct);
}
