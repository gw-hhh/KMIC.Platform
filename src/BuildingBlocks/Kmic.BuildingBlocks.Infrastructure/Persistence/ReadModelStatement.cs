using System.Data.Common;
using Kmic.BuildingBlocks.Application.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 一条读模型写入语句要用的表名、列名和参数
/// </summary>
/// <remarks>
/// 表名、列名和值的转换都取自 EF 模型，参数按列的类型映射创建。
/// 不带行时只用来按列名查询或批量更新，带行时写入这一行的数据列
/// </remarks>
/// <param name="entity">读模型的 EF 实体类型</param>
/// <param name="factory">只用来按 EF 的类型映射创建参数，不执行</param>
internal sealed class ReadModelStatement(IEntityType entity, DbCommand factory) : IAsyncDisposable
{
    // 这几列由框架维护，不取行上的值
    private static readonly string[] _managed =
        [nameof(IReadModel.IsTerminal), nameof(IReadModel.TerminalReason), nameof(IReadModel.TerminalAt), nameof(IReadModel.RebuildRunId), nameof(IReadModel.SyncedAt)];

    private readonly List<DbParameter> _parameters = [];

    /// <summary>
    /// 写入一行：数据列和它们的值取自这一行
    /// </summary>
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
