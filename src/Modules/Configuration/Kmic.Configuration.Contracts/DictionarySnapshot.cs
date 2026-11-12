using System.ComponentModel;

namespace Kmic.Configuration.Contracts;

/// <summary>
/// 一个字典类型的快照：类型信息、版本号和全部字典项（含停用的）
/// </summary>
/// <remarks>
/// 不可变，进缓存，所以必须是能直接序列化的简单记录
/// </remarks>
/// <param name="TypeCode">类型编码</param>
/// <param name="DisplayName">类型显示名</param>
/// <param name="IsHierarchical">是否层级字典</param>
/// <param name="Version">类型的版本号，任何字典项的变化都会递增它</param>
/// <param name="Items">全部字典项，按父项和次序排好</param>
[ImmutableObject(true)]
public sealed record DictionarySnapshot(string TypeCode, string DisplayName, bool IsHierarchical, long Version, IReadOnlyList<DictionaryEntry> Items);
