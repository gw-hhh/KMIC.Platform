using System.Data.Common;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在命令事务之外、用自己的连接和事务写数据库
/// </summary>
/// <remarks>
/// 命令返回失败结果时整个事务回滚，但有些记录必须保留：登录失败的计数、重复使用令牌的处理这类“失败本身就是要记下的事实”。
/// 它们用这里的独立事务提交，不受命令回滚影响。连接取自读池，不占事务池，所以命令占满事务池时也不会互相卡死。
/// 注意：独立事务看不到命令事务里还没提交的数据，也不能去写命令事务已经锁住的行，否则会等到命令结束，形成死锁
/// </remarks>
public interface IIndependentDatabase
{
    /// <summary>
    /// 开一个新连接和事务，执行完提交；出异常回滚
    /// </summary>
    /// <param name="work">要做的事，参数是已开启事务的连接与事务</param>
    /// <param name="ct">取消令牌</param>
    Task<TResult> InTransactionAsync<TResult>(Func<DbConnection, DbTransaction, CancellationToken, Task<TResult>> work, CancellationToken ct);
}
