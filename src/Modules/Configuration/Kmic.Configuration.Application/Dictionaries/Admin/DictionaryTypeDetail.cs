using System.Text.Json.Serialization;
using Kmic.Configuration.Contracts;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>字典类型详情（含全部字典项）</summary>
public sealed record DictionaryTypeDetail(
    string Code,
    string DisplayName,
    string OwnerModule,
    bool IsHierarchical,
    DictionaryTypeStatus Status,
    [property: JsonPropertyName("version")] long ConcurrencyVersion,
    IReadOnlyList<DictionaryEntry> Items);
