using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Exports;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

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
            throw new InvalidOperationException("导出仅限在后台作业中调用（流式连接池仅 Worker 可用，避免在请求中长时间占用连接）。");
        }

        var builder = new DbContextOptionsBuilder<TQueryContext>();
        PersistenceServiceCollectionExtensions.AddFrameworkExtensions(services, builder.UseNpgsql(dataSources.Stream));
        await using var context = ActivatorUtilities.CreateInstance<TQueryContext>(services, builder.Options);

        // 多读取一行用于判断是否超出最大导出行数限制
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
