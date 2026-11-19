using System.Text.Json.Serialization;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>
/// 字典类型列表项
/// </summary>
/// <param name="Code">类型编码</param>
/// <param name="DisplayName">显示名</param>
/// <param name="OwnerModule">声明字典类型的模块</param>
/// <param name="IsHierarchical">是否层级字典</param>
/// <param name="Status">状态</param>
/// <param name="ItemCount">字典项总数</param>
/// <param name="ActiveItemCount">启用的字典项数</param>
/// <param name="ConcurrencyVersion">版本号</param>
public sealed record DictionaryTypeListItem(
    string Code,
    string DisplayName,
    string OwnerModule,
    bool IsHierarchical,
    DictionaryTypeStatus Status,
    int ItemCount,
    int ActiveItemCount,
    [property: JsonPropertyName("version")] long ConcurrencyVersion);
