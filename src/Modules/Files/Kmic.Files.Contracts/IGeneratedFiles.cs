using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>写入并登记模块生成的文件（登记为异步操作，完成前可能无法查询到该文件）</summary>
public interface IGeneratedFiles
{
    /// <summary>将文件内容写入存储，不写入数据库</summary>
    /// <returns>写入的文件信息；内容为空、类型无法识别或大小超限时返回对应错误</returns>
    Task<Result<GeneratedFile>> WriteAsync(string source, string fileName, Stream content, TimeSpan retention, CancellationToken ct);

    /// <summary>在命令事务中请求登记已写入的文件</summary>
    void Register(GeneratedFile file);
}
