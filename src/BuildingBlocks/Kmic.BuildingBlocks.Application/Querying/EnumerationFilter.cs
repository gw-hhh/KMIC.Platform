using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 列表按枚举编码筛选：把请求里的编码换成枚举值
/// </summary>
/// <remarks>
/// 枚举在库里存的是编码，EF 只能拿枚举值和列比较，翻译不了 <c>Status.Code</c> 这样的属性访问，
/// 所以先在这里换成枚举值，查询里写 <c>values.Contains(x.Status)</c>。编码不认识时按字段错误返回，不当作“没有匹配”
/// </remarks>
public static class EnumerationFilter
{
    /// <summary>
    /// 把编码换成枚举值；没有给编码时得到空列表，表示不按这个字段筛选
    /// </summary>
    /// <typeparam name="TEnumeration">枚举类型</typeparam>
    /// <param name="codes">请求里的编码</param>
    /// <param name="field">请求里的字段名，错误里用它定位</param>
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
