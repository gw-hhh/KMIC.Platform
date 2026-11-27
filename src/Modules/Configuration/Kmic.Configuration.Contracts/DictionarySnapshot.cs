using System.ComponentModel;

namespace Kmic.Configuration.Contracts;

/// <summary>字典类型快照，包含类型信息、版本号及全部字典项（含已停用项）</summary>
[ImmutableObject(true)]
public sealed record DictionarySnapshot(string TypeCode, string DisplayName, bool IsHierarchical, long Version, IReadOnlyList<DictionaryEntry> Items);
