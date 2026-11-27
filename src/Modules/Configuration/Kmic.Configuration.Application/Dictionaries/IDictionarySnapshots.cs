using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries;

/// <summary>读取字典快照（缓存由基础设施管理，变更提交后按标签失效）</summary>
public interface IDictionarySnapshots
{
    Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct);
}
