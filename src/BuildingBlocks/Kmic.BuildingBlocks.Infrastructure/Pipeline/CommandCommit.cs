using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 提交事务
/// </summary>
internal static class CommandCommit
{
    /// <summary>
    /// 提交，不能取消：提交发出后再取消只会让结果变成未知
    /// </summary>
    /// <exception cref="CommitOutcomeUnknownException">提交没有得到确认</exception>
    public static async Task CommitAsync(IDbContextTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        try
        {
            await transaction.CommitAsync(CancellationToken.None);
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException or IOException)
        {
            throw new CommitOutcomeUnknownException("提交没有得到确认，结果未知。", exception);
        }
    }
}
