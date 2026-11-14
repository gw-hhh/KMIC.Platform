namespace Kmic.Identity.Application;

/// <summary>
/// 供命令读取跨聚合判断所需的数据
/// </summary>
/// <remarks>
/// 与仓储共用上下文和事务，须在取得相应咨询锁后读取，才能纳入此前已提交的并发变更。
/// 用于检查其他管理员是否存在、组织下是否有启用用户等跨聚合事实，不得改用查询侧的 <see cref="IIdentityQueryDb" />
/// </remarks>
public interface IIdentityCommandReads : IIdentityReads;
