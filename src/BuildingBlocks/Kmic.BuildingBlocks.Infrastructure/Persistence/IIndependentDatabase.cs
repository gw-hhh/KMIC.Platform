using System.Data.Common;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在命令事务之外用独立事务写数据库
/// </summary>
/// <remarks>
/// 看不到命令事务未提交的数据，也不能写命令已锁住的行，否则死锁
/// </remarks>
public interface IIndependentDatabase
{
    /// <summary>
    /// 开一个新连接和事务，执行完提交；出异常回滚
    /// </summary>
    Task<TResult> InTransactionAsync<TResult>(Func<DbConnection, DbTransaction, CancellationToken, Task<TResult>> work, CancellationToken ct);
}
