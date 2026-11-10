namespace Kmic.BuildingBlocks.Application.Security;

/// <summary>
/// 写安全事件：独立于业务事务，业务回滚后仍然保留（ADR-0018 第 4 条）
/// </summary>
public interface ISecurityEventWriter
{
    /// <summary>
    /// 放入写入队列；不等待、不抛异常，队列满时丢弃并告警
    /// </summary>
    void Write(SecurityEvent securityEvent);
}
