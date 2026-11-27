using System.Data.Common;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>在命令事务之外开启独立事务（不可见当前未提交数据，且严禁修改当前命令已锁定的行以免死锁）</summary>
public interface IIndependentDatabase
{
    /// <summary>开启新连接与独立事务执行操作，正常完成提交、异常回滚</summary>
    Task<TResult> InTransactionAsync<TResult>(Func<DbConnection, DbTransaction, CancellationToken, Task<TResult>> work, CancellationToken ct);
}
