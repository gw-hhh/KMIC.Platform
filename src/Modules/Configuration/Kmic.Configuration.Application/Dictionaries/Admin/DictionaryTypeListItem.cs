using System.Text.Json.Serialization;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>字典类型列表项</summary>
/// <param name="ItemCount">字典项总数</param>
/// <param name="ActiveItemCount">启用的字典项数</param>
public sealed record DictionaryTypeListItem(
    string Code,
    string DisplayName,
    string OwnerModule,
    bool IsHierarchical,
    DictionaryTypeStatus Status,
    int ItemCount,
    int ActiveItemCount,
    [property: JsonPropertyName("version")] long ConcurrencyVersion);
