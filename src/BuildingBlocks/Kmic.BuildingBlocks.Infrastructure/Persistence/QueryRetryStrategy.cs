using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 查询上下文的重试策略：连接断开、主备切换等暂时性故障最多重试 2 次，单次退避不超过 1 秒
/// </summary>
/// <remarks>
/// 借连接超时不重试：它说明应用自己的连接池已经饱和，重试只会让请求排得更久，应尽快返回 503 由前端稍后再试。
/// 命令不用任何重试策略，由客户端带同一幂等键重试（05-02 D4）
/// </remarks>
internal sealed class QueryRetryStrategy(ExecutionStrategyDependencies dependencies)
    : NpgsqlRetryingExecutionStrategy(dependencies, maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(1), errorCodesToAdd: null)
{
    /// <summary>
    /// 是否为借连接超时
    /// </summary>
    /// <remarks>
    /// Npgsql 没有专门的异常类型，只能按消息识别；测试固定了这个场景，升级 Npgsql 后消息一变测试就失败（实施待办 T-22）
    /// </remarks>
    public static bool IsPoolExhausted(Exception? exception)
    {
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
