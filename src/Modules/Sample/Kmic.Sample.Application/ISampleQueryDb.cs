using Kmic.Sample.Domain.SpecimenCategories;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application;

/// <summary>Sample 模块查询入口，已应用数据范围和软删除过滤</summary>
public interface ISampleQueryDb
{
    IQueryable<Specimen> Specimens { get; }

    /// <summary>
    /// 示例分类
    /// </summary>
    IQueryable<SpecimenCategory> SpecimenCategories { get; }
}
