using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 读模型写入
/// </summary>
/// <remarks>
/// 三种写入都是一条 INSERT … ON CONFLICT DO UPDATE … WHERE NOT "IsTerminal"：并发时后到的事务等先到的提交，
/// 再按最新的行重新判断，所以迟到的旧快照不会把终态改回来。表名、列名和值的转换都取自 EF 模型
/// </remarks>
internal sealed class ReadModelWriter<T>(ModuleWriteContexts contexts) : IReadModelWriter<T>, IReadModelMaintenance<T>
    where T : class, IReadModel
{
    public async Task<bool> UpsertAsync(T row, CancellationToken ct)
    {
        var (context, statement) = Prepare(row);
        await using (statement)
        {
            return await ExecuteAsync(context, statement, $"""
                INSERT INTO {statement.Table} AS r ({statement.Columns}, {statement.Terminal}, {statement.Synced})
                VALUES ({statement.Values}, false, clock_timestamp())
                ON CONFLICT ({statement.Id}) DO UPDATE SET {statement.Overwrite}, {statement.Synced} = EXCLUDED.{statement.Synced}
                WHERE NOT r.{statement.Terminal} AND r.{statement.Version} < EXCLUDED.{statement.Version}
                """, ct);
        }
    }

    public async Task<bool> PurgeAsync(T row, CancellationToken ct)
    {
        var (context, statement) = Prepare(row);
        await using (statement)
        {
            var reason = statement.Column(nameof(IReadModel.TerminalReason));
            var at = statement.Column(nameof(IReadModel.TerminalAt));
            var status = statement.Column(nameof(IReadModel.Status));
            var purged = statement.Parameter(nameof(IReadModel.TerminalReason), ReadModelTerminalReason.PurgeEvent);
            return await ExecuteAsync(context, statement, $"""
                INSERT INTO {statement.Table} AS r ({statement.Columns}, {statement.Terminal}, {reason}, {at}, {statement.Synced})
                VALUES ({statement.Values}, true, {purged}, clock_timestamp(), clock_timestamp())
                ON CONFLICT ({statement.Id}) DO UPDATE SET {statement.Terminal} = true, {reason} = EXCLUDED.{reason}, {at} = EXCLUDED.{at},
                    {statement.Version} = EXCLUDED.{statement.Version}, {status} = EXCLUDED.{status}, {statement.Synced} = EXCLUDED.{statement.Synced}
                WHERE NOT r.{statement.Terminal}
                """, ct);
        }
    }

    public async Task<bool> UpsertFromScanAsync(T row, Guid runId, CancellationToken ct)
    {
        var (context, statement) = Prepare(row);
        await using (statement)
        {
            // 本地版本不高于扫描版本时覆盖全部字段，否则只记轮次，版本不会倒退
            var run = statement.Column(nameof(IReadModel.RebuildRunId));
            var runValue = statement.Parameter(nameof(IReadModel.RebuildRunId), runId);
            var version = statement.Version;
            var others = statement.Updatable.Where(c => !string.Equals(c, version, StringComparison.Ordinal))
                .Select(c => $"{c} = CASE WHEN r.{version} <= EXCLUDED.{version} THEN EXCLUDED.{c} ELSE r.{c} END");
            return await ExecuteAsync(context, statement, $"""
                INSERT INTO {statement.Table} AS r ({statement.Columns}, {statement.Terminal}, {run}, {statement.Synced})
                VALUES ({statement.Values}, false, {runValue}, clock_timestamp())
                ON CONFLICT ({statement.Id}) DO UPDATE SET {version} = GREATEST(r.{version}, EXCLUDED.{version}),
                    {string.Join(", ", others)}, {run} = EXCLUDED.{run}, {statement.Synced} = EXCLUDED.{statement.Synced}
                WHERE NOT r.{statement.Terminal}
                """, ct);
        }
    }

    public async Task<long> CountAsync(CancellationToken ct)
    {
        var (context, statement) = Prepare();
        await using (statement)
        {
            var sql = $"""
                SELECT count(*) AS "Value" FROM {statement.Table}
                """;
            return await context.Database.SqlQueryRaw<long>(sql).SingleAsync(ct);
        }
    }

    public async Task<IReadOnlyList<ReadModelCandidate>> ListCandidatesAsync(Guid runId, Guid? after, int size, CancellationToken ct)
    {
        var (context, statement) = Prepare();
        await using (statement)
        {
            var run = statement.Column(nameof(IReadModel.RebuildRunId));
            var runValue = statement.Parameter(nameof(IReadModel.RebuildRunId), runId);
            var afterValue = statement.Parameter(nameof(IReadModel.Id), after);
            var sql = $"""
                SELECT {statement.Id} AS "Id", {statement.Version} AS "SourceVersion" FROM {statement.Table}
                WHERE NOT {statement.Terminal} AND ({run} IS NULL OR {run} <> {runValue}) AND ({afterValue} IS NULL OR {statement.Id} > {afterValue})
                ORDER BY {statement.Id}
                LIMIT {size}
                """;
            return await context.Database.SqlQueryRaw<ReadModelCandidate>(sql, [.. statement.Parameters]).ToListAsync(ct);
        }
    }

    public async Task<int> MarkMissingAsync(IReadOnlyCollection<ReadModelCandidate> rows, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var (context, statement) = Prepare();
        await using (statement)
        {
            var reason = statement.Column(nameof(IReadModel.TerminalReason));
            var at = statement.Column(nameof(IReadModel.TerminalAt));
            var confirmed = statement.Parameter(nameof(IReadModel.TerminalReason), ReadModelTerminalReason.ConfirmedMissing);

            // 版本也要对得上：选为候选之后被实时事件更新过的行不动
            var sql = $"""
                UPDATE {statement.Table} AS r
                SET {statement.Terminal} = true, {reason} = {confirmed}, {at} = clock_timestamp(), {statement.Synced} = clock_timestamp()
                FROM unnest(@ids, @versions) AS m("Id", "SourceVersion")
                WHERE r.{statement.Id} = m."Id" AND r.{statement.Version} = m."SourceVersion" AND NOT r.{statement.Terminal}
                """;
            object[] parameters =
            [
                .. statement.Parameters,
                new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = rows.Select(r => r.Id).ToArray() },
                new NpgsqlParameter("versions", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = rows.Select(r => r.SourceVersion).ToArray() },
            ];
            return await context.Database.ExecuteSqlRawAsync(sql, parameters, ct);
        }
    }

    private static async Task<bool> ExecuteAsync(ModuleDbContext context, ReadModelStatement statement, string sql, CancellationToken ct) =>
        await context.Database.ExecuteSqlRawAsync(sql, statement.Parameters, ct) == 1;

    private (ModuleDbContext Context, ReadModelStatement Statement) Prepare(T row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var (context, entity) = Entity();
        return (context, new ReadModelStatement(entity, context.Database.GetDbConnection().CreateCommand(), row));
    }

    private (ModuleDbContext Context, ReadModelStatement Statement) Prepare()
    {
        var (context, entity) = Entity();
        return (context, new ReadModelStatement(entity, context.Database.GetDbConnection().CreateCommand()));
    }

    private (ModuleDbContext Context, IEntityType Entity) Entity()
    {
        var context = contexts.Get(ModuleNames.Of(typeof(T)));
        var entity = context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} 没有映射到 {context.Module} 模块的上下文里。");
        return (context, entity);
    }
}
