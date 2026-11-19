using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 写入并请求登记模块生成的文件
/// </summary>
/// <remarks>
/// 登记是异步的，完成前可能查不到
/// </remarks>
public interface IGeneratedFiles
{
    /// <summary>
    /// 写入文件内容，不写数据库
    /// </summary>
    /// <returns>写入的文件信息；内容为空、类型无法识别或大小超限时返回对应错误</returns>
    Task<Result<GeneratedFile>> WriteAsync(string source, string fileName, Stream content, TimeSpan retention, CancellationToken ct);

    /// <summary>
    /// 在命令事务中请求登记已写入的文件
    /// </summary>
    void Register(GeneratedFile file);
}
