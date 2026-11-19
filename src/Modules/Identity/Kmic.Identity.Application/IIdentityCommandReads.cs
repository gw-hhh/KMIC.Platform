namespace Kmic.Identity.Application;

/// <summary>
/// 命令读取跨聚合事实用的接口，与仓储共用上下文和事务
/// </summary>
/// <remarks>
/// 需要串行检查时先取锁再读；不要换成查询侧的 <see cref="IIdentityQueryDb" />
/// </remarks>
public interface IIdentityCommandReads : IIdentityReads;
