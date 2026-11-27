using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed class QueryRetryStrategy(ExecutionStrategyDependencies dependencies)
    : NpgsqlRetryingExecutionStrategy(dependencies, maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(1), errorCodesToAdd: null)
{
    public static bool IsPoolExhausted(Exception? exception)
    {
        // Npgsql 未对此场景提供独立异常类型，仅能通过异常消息识别；已有测试覆盖该消息格式，升级 Npgsql 版本时需注意兼容性
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is NpgsqlException && current.Message.StartsWith("The connection pool has been exhausted", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    protected override bool ShouldRetryOn(Exception? exception) => !IsPoolExhausted(exception) && base.ShouldRetryOn(exception);
}
