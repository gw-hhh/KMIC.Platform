using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 写入并请求登记模块生成的文件
/// </summary>
/// <remarks>
/// 先调用 WriteAsync 写入存储，再在业务命令事务中调用 Register 请求登记。
/// 登记请求随业务数据写入发件箱，事务失败则不发布；未登记的存储对象由对账清理。
/// 登记异步完成，完成前可能查询不到文件
/// </remarks>
public interface IGeneratedFiles
{
    /// <summary>
    /// 把内容写进存储，不碰数据库
    /// </summary>
    /// <param name="source">生成方，如 Operations.AuditExport；登记后的 Profile 为 Generated:来源</param>
    /// <param name="fileName">文件名，只用它的扩展名识别类型</param>
    /// <param name="content">文件内容，读到结尾为止</param>
    /// <param name="retention">从开始写入起计算的保留时长；到期且未关联时可清理</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>写入的文件；内容是空的、类型不认识、超过大小上限时返回对应的错误</returns>
    Task<Result<GeneratedFile>> WriteAsync(string source, string fileName, Stream content, TimeSpan retention, CancellationToken ct);

    /// <summary>
    /// 请求登记一个写好的文件，只能在命令里调用
    /// </summary>
    /// <param name="file">WriteAsync 返回的文件</param>
    void Register(GeneratedFile file);
}
