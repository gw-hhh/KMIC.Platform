using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// <see cref="IDataScopeSql"/> 的实现：和 <c>DataScopeFilters</c> 读同一个范围，逻辑一一对应
/// </summary>
internal sealed partial class DataScopeSql(IDataScope scope) : IDataScopeSql
{
    // 列或“别名.列”，各段可以带双引号；只接受标识符，防止把别的东西拼进 SQL
    [GeneratedRegex(@"^(?:[A-Za-z_][A-Za-z0-9_]*|""[A-Za-z_][A-Za-z0-9_]*"")(?:\.(?:[A-Za-z_][A-Za-z0-9_]*|""[A-Za-z_][A-Za-z0-9_]*""))?$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex ColumnPattern();

    public FormattableString Condition(string scopeObject, string? ownerColumn, string? orgUnitColumn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeObject);
        ValidateColumn(ownerColumn, nameof(ownerColumn));
        ValidateColumn(orgUnitColumn, nameof(orgUnitColumn));

        // 和过滤器一样：不受限，或范围等级是全部，都不加限制
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

        // 组织列总是加：范围里没有组织时数组为空，条件不成立，和过滤器的 Contains 一致
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
            throw new ArgumentException($"列写法不对：{column}。只能是“列”或“别名.列”，可以带双引号。", parameterName);
        }
    }
}
