using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Exports;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在流式池上为导出单独建一个查询上下文
/// </summary>
internal sealed class ExportReader<TQueryDb, TQueryContext>(
    IServiceProvider services, DataSources dataSources, IExecutionContext execution, IOptions<JobOptions> options) : IExportReader<TQueryDb>
    where TQueryDb : class
    where TQueryContext : ModuleQueryDbContext, TQueryDb
{
    public async Task<Result<long>> ReadAsync<T>(Func<TQueryDb, IQueryable<T>> query, Func<T, CancellationToken, ValueTask> write, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(write);
        if (execution.Origin != CallOrigin.BackgroundJob)
        {
            throw new InvalidOperationException("导出入口只能在后台作业里用：流式连接只有 Worker 有，请求里导出会长时间占着连接。");
        }

        var builder = new DbContextOptionsBuilder<TQueryContext>();
        PersistenceServiceCollectionExtensions.AddFrameworkExtensions(services, builder.UseNpgsql(dataSources.Stream));
        await using var context = ActivatorUtilities.CreateInstance<TQueryContext>(services, builder.Options);

        // 多取一行，读到它就说明超了上限
        var limit = options.Value.ExportMaxRows;
        long count = 0;
        await foreach (var row in query(context).Take(limit + 1).AsAsyncEnumerable().WithCancellation(ct))
        {
            if (++count > limit)
            {
                return CommonErrors.ExportTooLarge;
            }

            await write(row, ct);
        }

        return count;
    }
}
