using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 查询重试策略
/// </summary>
internal sealed class QueryRetryStrategy(ExecutionStrategyDependencies dependencies)
    : NpgsqlRetryingExecutionStrategy(dependencies, maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(1), errorCodesToAdd: null)
{
    /// <summary>
    /// 是否连接池耗尽
    /// </summary>
    public static bool IsPoolExhausted(Exception? exception)
    {
        // Npgsql 没有专门的异常类型，只能按消息判断。有测试盯着这条消息，升级 Npgsql 后消息变了测试会失败
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is NpgsqlException && current.Message.StartsWith("The connection pool has been exhausted", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    protected override bool ShouldRetryOn(Exception? exception) => !IsPoolExhausted(exception) && base.ShouldRetryOn(exception);
}
