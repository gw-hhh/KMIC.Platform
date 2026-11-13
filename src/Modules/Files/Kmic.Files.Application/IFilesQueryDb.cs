namespace Kmic.Files.Application;

/// <summary>
/// Files 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器只从它出发：已经带了数据范围和软删除过滤，也不会读到命令里还没保存的修改。
/// 每个聚合加一个 IQueryable 属性
/// </remarks>
public interface IFilesQueryDb;
