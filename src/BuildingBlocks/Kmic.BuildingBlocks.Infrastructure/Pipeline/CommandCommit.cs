using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

internal static class CommandCommit
{
    // 不可取消：提交请求发出后取消只会导致事务结果未知
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
