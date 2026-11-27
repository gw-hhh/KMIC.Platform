using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>列表枚举筛选辅助工具，将编码列表转换为强类型枚举值后可直接用于 <c>values.Contains(x.Status)</c>。</summary>
public static class EnumerationFilter
{
    public static Result<IReadOnlyList<TEnumeration>> Parse<TEnumeration>(IReadOnlyList<string>? codes, string field)
        where TEnumeration : Enumeration<TEnumeration>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);

        if (codes is null or { Count: 0 })
        {
            return Array.Empty<TEnumeration>();
        }

        var values = new List<TEnumeration>(codes.Count);
        foreach (var code in codes)
        {
            if (!Enumeration<TEnumeration>.TryFromCode(code, out var value))
            {
                return CommonErrors.ValidationFailedWith([new FieldError(field, "Invalid", $"“{code}”不是有效的取值")]);
            }

            values.Add(value);
        }

        return values;
    }
}
