using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 模块自己生成的文件（导出结果、系统生成的 PDF 等）的入口，分两步
/// </summary>
/// <remarks>
/// 后台作业的处理器不在事务里，所以写入和登记分开：处理器先 <see cref="WriteAsync" /> 把内容写进存储，
/// 再发一个命令，在命令里 <see cref="Register" /> 请求登记。登记请求和命令的业务数据在同一个事务里进发件箱，
/// 命令失败请求就不发，留在存储里的对象由对账清理。从写入到登记完成有几秒间隔，这期间下载会得到“还没准备好”
/// </remarks>
public interface IGeneratedFiles
{
    /// <summary>
    /// 把内容写进存储，不碰数据库
    /// </summary>
    /// <param name="source">生成方，如 Operations.AuditExport，登记后文件的上传档案是 Generated:来源</param>
    /// <param name="fileName">文件名，只用它的扩展名识别类型</param>
    /// <param name="content">文件内容，读到结尾为止</param>
    /// <param name="retention">登记之后多久没有被业务对象关联就清理</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>写入的文件；内容是空的、类型不认识、超过大小上限时返回对应的错误</returns>
    Task<Result<GeneratedFile>> WriteAsync(string source, string fileName, Stream content, TimeSpan retention, CancellationToken ct);

    /// <summary>
    /// 请求登记一个写好的文件，只能在命令里调用
    /// </summary>
    /// <param name="file">WriteAsync 返回的文件</param>
    void Register(GeneratedFile file);
}
