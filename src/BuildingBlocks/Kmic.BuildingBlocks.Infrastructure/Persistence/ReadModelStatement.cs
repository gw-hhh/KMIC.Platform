using System.Data.Common;
using Kmic.BuildingBlocks.Application.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed class ReadModelStatement(IEntityType entity, DbCommand factory) : IAsyncDisposable
{
    // 以下系统列由框架自动维护，不从业务行数据中读取
    private static readonly string[] _managed =
        [nameof(IReadModel.IsTerminal), nameof(IReadModel.TerminalReason), nameof(IReadModel.TerminalAt), nameof(IReadModel.RebuildRunId), nameof(IReadModel.SyncedAt)];

    private readonly List<DbParameter> _parameters = [];

    public ReadModelStatement(IEntityType entity, DbCommand factory, object row)
        : this(entity, factory)
    {
        var data = entity.GetProperties().Where(p => !_managed.Contains(p.Name, StringComparer.Ordinal)).ToList();
        Columns = string.Join(", ", data.Select(ColumnOf));
        Values = string.Join(", ", data.Select(p => Parameter(p, p.PropertyInfo?.GetValue(row))));
        Updatable = [.. data.Where(p => !string.Equals(p.Name, nameof(IReadModel.Id), StringComparison.Ordinal)).Select(ColumnOf)];
    }

    public string Table => $"\"{entity.GetSchema()}\".\"{entity.GetTableName()}\"";

    public string Columns { get; } = "";

    public string Values { get; } = "";

    public IReadOnlyList<string> Updatable { get; } = [];

    public string Overwrite => string.Join(", ", Updatable.Select(c => $"{c} = EXCLUDED.{c}"));

    public string Id => Column(nameof(IReadModel.Id));

    public string Version => Column(nameof(IReadModel.SourceVersion));

    public string Terminal => Column(nameof(IReadModel.IsTerminal));

    public string Synced => Column(nameof(IReadModel.SyncedAt));

    public IReadOnlyList<DbParameter> Parameters => _parameters;

    public string Column(string property) => ColumnOf(Property(property));

    public string Parameter(string property, object? value) => Parameter(Property(property), value);

    public ValueTask DisposeAsync() => factory.DisposeAsync();

    private IProperty Property(string name) =>
        entity.FindProperty(name) ?? throw new InvalidOperationException($"{entity.ClrType.Name} 没有映射 {name}。");

    private string ColumnOf(IProperty property) => $"\"{property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName() ?? entity.ClrType.Name, entity.GetSchema()))}\"";

    private string Parameter(IProperty property, object? value)
    {
        var name = $"p{_parameters.Count}";
        _parameters.Add(property.GetRelationalTypeMapping().CreateParameter(factory, name, value, property.IsNullable));
        return "@" + name;
    }
}
