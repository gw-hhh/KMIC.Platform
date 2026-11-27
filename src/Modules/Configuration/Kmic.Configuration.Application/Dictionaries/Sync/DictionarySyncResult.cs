namespace Kmic.Configuration.Application.Dictionaries.Sync;

/// <summary>一次字典类型同步的结果</summary>
/// <param name="Registered">新登记的类型数</param>
/// <param name="Updated">已更新类型数（含显示名、层级属性或状态变化）</param>
/// <param name="Retired">新标记为已废弃的类型数</param>
public sealed record DictionarySyncResult(int Registered, int Updated, int Retired);
