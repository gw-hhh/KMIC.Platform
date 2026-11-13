using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application;

/// <summary>
/// Files 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器只从它出发：不跟踪实体，也不会读到命令里还没保存的修改。文件元数据不声明数据范围，
/// 谁能看由权限点决定；业务下载的权限完全继承业务对象
/// </remarks>
public interface IFilesQueryDb
{
    /// <summary>
    /// 文件，带着引用
    /// </summary>
    IQueryable<StoredFile> Files { get; }
}
