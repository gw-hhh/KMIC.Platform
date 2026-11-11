using System.Data.Common;
using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 读模型写入
/// </summary>
/// <remarks>
/// 三种写入都是一条 INSERT … ON CONFLICT DO UPDATE … WHERE NOT "IsTerminal"：并发时后到的事务等先到的提交，
/// 再按最新的行重新判断，所以迟到的旧快照不会把终态改回来。表名、列名和值的转换都取自 EF 模型
/// </remarks>
internal sealed class ReadModelWriter<T>(ModuleWriteContexts contexts) : IReadModelWriter<T>
    where T : class, IReadModel
{
    // 这几列由写入器自己维护，不取行上的值
    private static readonly string[] _managed =
        [nameof(IReadModel.IsTerminal), nameof(IReadModel.TerminalReason), nameof(IReadModel.TerminalAt), nameof(IReadModel.RebuildRunId), nameof(IReadModel.SyncedAt)];

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

    private static async Task<bool> ExecuteAsync(ModuleDbContext context, Statement statement, string sql, CancellationToken ct) =>
        await context.Database.ExecuteSqlRawAsync(sql, statement.Parameters, ct) == 1;

    private (ModuleDbContext Context, Statement Statement) Prepare(T row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var context = contexts.Get(ModuleNames.Of(typeof(T)));
        var entity = context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} 没有映射到 {context.Module} 模块的上下文里。");
        return (context, new Statement(entity, context.Database.GetDbConnection().CreateCommand(), row));
    }

    /// <summary>
    /// 一条写入语句的列和参数
    /// </summary>
    private sealed class Statement : IAsyncDisposable
    {
        private readonly IEntityType _entity;
        private readonly StoreObjectIdentifier _table;

        // 只用来按 EF 的类型映射创建参数，不执行
        private readonly DbCommand _factory;
        private readonly List<DbParameter> _parameters = [];

        public Statement(IEntityType entity, DbCommand factory, T row)
        {
            _entity = entity;
            _factory = factory;
            _table = StoreObjectIdentifier.Table(entity.GetTableName() ?? entity.ClrType.Name, entity.GetSchema());

            var data = entity.GetProperties().Where(p => !_managed.Contains(p.Name, StringComparer.Ordinal)).ToList();
            Columns = string.Join(", ", data.Select(ColumnOf));
            Values = string.Join(", ", data.Select(p => Parameter(p, p.PropertyInfo?.GetValue(row))));
            Updatable = [.. data.Where(p => !string.Equals(p.Name, nameof(IReadModel.Id), StringComparison.Ordinal)).Select(ColumnOf)];
        }

        public string Table => $"\"{_entity.GetSchema()}\".\"{_entity.GetTableName()}\"";

        public string Columns { get; }

        public string Values { get; }

        public IReadOnlyList<string> Updatable { get; }

        public string Overwrite => string.Join(", ", Updatable.Select(c => $"{c} = EXCLUDED.{c}"));

        public string Id => Column(nameof(IReadModel.Id));

        public string Version => Column(nameof(IReadModel.SourceVersion));

        public string Terminal => Column(nameof(IReadModel.IsTerminal));

        public string Synced => Column(nameof(IReadModel.SyncedAt));

        public IReadOnlyList<DbParameter> Parameters => _parameters;

        public string Column(string property) => ColumnOf(Property(property));

        public string Parameter(string property, object? value) => Parameter(Property(property), value);

        public ValueTask DisposeAsync() => _factory.DisposeAsync();

        private IProperty Property(string name) =>
            _entity.FindProperty(name) ?? throw new InvalidOperationException($"{_entity.ClrType.Name} 没有映射 {name}。");

        private string ColumnOf(IProperty property) => $"\"{property.GetColumnName(_table)}\"";

        private string Parameter(IProperty property, object? value)
        {
            var name = $"p{_parameters.Count}";
            _parameters.Add(property.GetRelationalTypeMapping().CreateParameter(_factory, name, value, property.IsNullable));
            return "@" + name;
        }
    }
}
