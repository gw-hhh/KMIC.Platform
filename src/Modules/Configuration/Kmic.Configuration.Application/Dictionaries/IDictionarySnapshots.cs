using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries;

/// <summary>
/// 读字典快照：走缓存，管理员改动之后按标签失效
/// </summary>
/// <remarks>
/// 缓存只能在基础设施里碰，所以读取放在这个接口后面，查询处理器和 Contracts 适配器都经它取
/// </remarks>
public interface IDictionarySnapshots
{
    /// <summary>
    /// 取一个字典类型的快照；类型不存在返回空
    /// </summary>
    Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct);
}
