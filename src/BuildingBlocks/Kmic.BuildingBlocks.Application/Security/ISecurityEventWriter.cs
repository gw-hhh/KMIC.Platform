namespace Kmic.BuildingBlocks.Application.Security;

/// <summary>
/// 安全事件写入
/// </summary>
/// <remarks>
/// 不在业务事务里，业务回滚后安全事件仍然保留
/// </remarks>
public interface ISecurityEventWriter
{
    /// <summary>
    /// 写入安全事件，不等待也不抛异常，队列满时丢弃并告警
    /// </summary>
    void Write(SecurityEvent securityEvent);
}
