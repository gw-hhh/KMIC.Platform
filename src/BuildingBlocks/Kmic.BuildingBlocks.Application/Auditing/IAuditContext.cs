namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>审计补充上下文。</summary>
public interface IAuditContext
{
    /// <summary>补充操作目标（如导出涉及的数据集）。</summary>
    void AddTarget(string resourceType, Guid resourceId);

    /// <summary>补充说明信息（如导出筛选条件），禁止写入敏感数据。</summary>
    void Add(string key, string value);
}
