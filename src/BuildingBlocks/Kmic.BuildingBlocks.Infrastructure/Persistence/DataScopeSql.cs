using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed partial class DataScopeSql(IDataScope scope) : IDataScopeSql
{
    // 仅允许“列”或“别名.列”格式的标识符（各段可带双引号），防止 SQL 注入
    [GeneratedRegex(@"^(?:[A-Za-z_][A-Za-z0-9_]*|""[A-Za-z_][A-Za-z0-9_]*"")(?:\.(?:[A-Za-z_][A-Za-z0-9_]*|""[A-Za-z_][A-Za-z0-9_]*""))?$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex ColumnPattern();

    public FormattableString Condition(string scopeObject, string? ownerColumn, string? orgUnitColumn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeObject);
        ValidateColumn(ownerColumn, nameof(ownerColumn));
        ValidateColumn(orgUnitColumn, nameof(orgUnitColumn));

        // 与实体过滤器保持一致：不受限或范围等级为“全部”时不附加过滤条件
        if (scope.Unrestricted || scope.For(scopeObject).All)
        {
            return FormattableStringFactory.Create("TRUE");
        }

        var effective = scope.For(scopeObject);
        var parts = new List<string>();
        var arguments = new List<object>();
        if (ownerColumn is not null && effective.Owner is { } owner)
        {
            parts.Add($"{ownerColumn} = {{{arguments.Count}}}");
            arguments.Add(owner.Value);
        }

        // 若指定了组织列则始终附加过滤条件：当范围中无可用组织时数组为空，条件恒为假，与实体过滤器的 Contains 语义一致
        if (orgUnitColumn is not null)
        {
            parts.Add($"{orgUnitColumn} = ANY({{{arguments.Count}}})");
            arguments.Add(effective.OrgUnits.Select(o => o.Value).ToArray());
        }

        return parts.Count == 0
            ? FormattableStringFactory.Create("FALSE")
            : FormattableStringFactory.Create($"({string.Join(" OR ", parts)})", [.. arguments]);
    }

    private static void ValidateColumn(string? column, string parameterName)
    {
        if (column is not null && !ColumnPattern().IsMatch(column))
        {
            throw new ArgumentException($"列名格式无效：{column}，仅支持“列”或“别名.列”（可带双引号）。", parameterName);
        }
    }
}
