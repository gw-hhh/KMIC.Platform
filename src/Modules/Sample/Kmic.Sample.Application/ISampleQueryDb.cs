using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application;

/// <summary>
/// Sample 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器只从它出发：已经带了数据范围和软删除过滤，也不会读到命令里还没保存的修改
/// </remarks>
public interface ISampleQueryDb
{
    /// <summary>
    /// 示例标本
    /// </summary>
    IQueryable<Specimen> Specimens { get; }
}
