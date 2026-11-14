using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application;

/// <summary>
/// Sample 模块的查询入口
/// </summary>
/// <remarks>
/// 查询已应用数据范围和软删除过滤，不返回命令中尚未保存的修改
/// </remarks>
public interface ISampleQueryDb
{
    /// <summary>
    /// 示例
    /// </summary>
    IQueryable<Specimen> Specimens { get; }
}
