using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application;

/// <summary>
/// Files 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器通过此接口读取，不跟踪实体，也不读取命令中尚未保存的修改。
/// 文件元数据按权限点管理，不声明数据范围；业务下载须通过业务对象授权
/// </remarks>
public interface IFilesQueryDb
{
    /// <summary>
    /// 文件及其引用
    /// </summary>
    IQueryable<StoredFile> Files { get; }

    /// <summary>
    /// 对账发现的存储问题
    /// </summary>
    IQueryable<StorageIssue> StorageIssues { get; }
}
