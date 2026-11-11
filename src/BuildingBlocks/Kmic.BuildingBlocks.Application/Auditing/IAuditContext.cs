namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>
/// 审计补充信息
/// </summary>
public interface IAuditContext
{
    /// <summary>
    /// 补充操作对象，如导出涉及的数据集
    /// </summary>
    void AddTarget(string resourceType, Guid resourceId);

    /// <summary>
    /// 补充说明，如导出的筛选条件。不要写入敏感数据
    /// </summary>
    void Add(string key, string value);
}
