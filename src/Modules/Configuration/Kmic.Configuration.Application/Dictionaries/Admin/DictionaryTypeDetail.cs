using System.Text.Json.Serialization;
using Kmic.Configuration.Contracts;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>
/// 字典类型详情
/// </summary>
/// <param name="Code">类型编码</param>
/// <param name="DisplayName">显示名</param>
/// <param name="OwnerModule">声明字典类型的模块</param>
/// <param name="IsHierarchical">是否层级字典</param>
/// <param name="Status">状态</param>
/// <param name="ConcurrencyVersion">字典类型版本号</param>
/// <param name="Items">全部字典项</param>
public sealed record DictionaryTypeDetail(
    string Code,
    string DisplayName,
    string OwnerModule,
    bool IsHierarchical,
    DictionaryTypeStatus Status,
    [property: JsonPropertyName("version")] long ConcurrencyVersion,
    IReadOnlyList<DictionaryEntry> Items);
