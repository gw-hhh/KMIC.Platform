using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries;

/// <summary>
/// 读取字典快照，缓存由基础设施管理，修改提交后按标签失效
/// </summary>
public interface IDictionarySnapshots
{
    /// <summary>
    /// 读取指定字典的快照，不存在时返回 null
    /// </summary>
    Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct);
}
