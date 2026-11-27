namespace Kmic.BuildingBlocks.Application.Security;

/// <summary>安全事件写入器；独立于业务事务之外，业务事务回滚后仍会保留。</summary>
public interface ISecurityEventWriter
{
    /// <summary>异步写入安全事件，不阻塞且不抛异常；队列满时丢弃并记录告警。</summary>
    void Write(SecurityEvent securityEvent);
}
