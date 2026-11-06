using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Spike15.Tests;

// 查询的重试策略（05-02 D4）：只重试真正的暂时性故障，次数和总等待都有上限；
// 借连接超时说明应用自身已饱和，重试只会让排队更长，直接失败（返回 503 retryable）
public sealed class QueryRetryStrategy(ExecutionStrategyDependencies dependencies)
    : NpgsqlRetryingExecutionStrategy(dependencies, maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(1), errorCodesToAdd: null)
{
    protected override bool ShouldRetryOn(Exception? exception) =>
        !IsPoolExhausted(exception) && base.ShouldRetryOn(exception);

    // Npgsql 没有单独的异常类型，只能按消息识别；测试固定了这段消息，升级 Npgsql 时会发现变化
    public static bool IsPoolExhausted(Exception? error)
    {
        for (var e = error; e is not null; e = e.InnerException)
        {
            if (e is NpgsqlException && e.Message.StartsWith("The connection pool has been exhausted", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
