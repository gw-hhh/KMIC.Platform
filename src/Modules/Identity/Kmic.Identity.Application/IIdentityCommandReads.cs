namespace Kmic.Identity.Application;

/// <summary>
/// 命令里为判断而读取其他数据的入口
/// </summary>
/// <remarks>
/// 和仓储用同一个上下文、同一个事务：读到的是这个事务里的最新状态，加了咨询锁之后再读，看到的就是别的事务已经提交的结果。
/// 命令里要判断“还有没有别的管理员”“组织单元是否有启用的用户”这类跨聚合的事实时用它，不用查询侧的 <see cref="IIdentityQueryDb" />
/// </remarks>
public interface IIdentityCommandReads : IIdentityReads;
