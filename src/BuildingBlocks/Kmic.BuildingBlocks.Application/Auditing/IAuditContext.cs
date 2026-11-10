namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>
/// 命令向本次审计事件补充信息；变更与被修改的聚合根由拦截器自动采集
/// </summary>
public interface IAuditContext
{
    /// <summary>
    /// 补充审计对象：没有被修改、但与操作相关的资源，如导出涉及的数据集
    /// </summary>
    void AddTarget(string resourceType, Guid resourceId);

    /// <summary>
    /// 补充说明，如导出的筛选条件、批量操作的条数；命令参数不会自动记录，敏感数据不要写进来
    /// </summary>
    void Add(string key, string value);
}
