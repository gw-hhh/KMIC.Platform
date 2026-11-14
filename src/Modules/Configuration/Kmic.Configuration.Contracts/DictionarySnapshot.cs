using System.ComponentModel;

namespace Kmic.Configuration.Contracts;

/// <summary>
/// 字典类型快照，包含类型信息、版本号及全部字典项（含已停用项）
/// </summary>
/// <remarks>
/// 用于缓存，须保持不可变且可序列化
/// </remarks>
/// <param name="TypeCode">类型编码</param>
/// <param name="DisplayName">类型显示名</param>
/// <param name="IsHierarchical">是否层级字典</param>
/// <param name="Version">类型的版本号，任何字典项的变化都会递增它</param>
/// <param name="Items">全部字典项，按父项和排序值排列</param>
[ImmutableObject(true)]
public sealed record DictionarySnapshot(string TypeCode, string DisplayName, bool IsHierarchical, long Version, IReadOnlyList<DictionaryEntry> Items);
