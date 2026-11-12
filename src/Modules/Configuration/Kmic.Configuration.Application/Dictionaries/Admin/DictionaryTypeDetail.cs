using System.Text.Json.Serialization;
using Kmic.Configuration.Contracts;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>
/// 字典类型详情
/// </summary>
/// <param name="Code">类型编码</param>
/// <param name="DisplayName">显示名</param>
/// <param name="OwnerModule">声明它的模块</param>
/// <param name="IsHierarchical">是否层级字典</param>
/// <param name="Status">状态：使用中、已退役</param>
/// <param name="ConcurrencyVersion">版本号，输出为 version；修改字典项时带上它做并发检查</param>
/// <param name="Items">全部字典项，按父项和次序排好</param>
public sealed record DictionaryTypeDetail(
    string Code,
    string DisplayName,
    string OwnerModule,
    bool IsHierarchical,
    DictionaryTypeStatus Status,
    [property: JsonPropertyName("version")] long ConcurrencyVersion,
    IReadOnlyList<DictionaryEntry> Items);
