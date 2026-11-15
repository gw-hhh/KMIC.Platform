using Kmic.BuildingBlocks.Application.Events;

namespace Kmic.Configuration.Application.Dictionaries;

/// <summary>
/// 登记字典变更后的缓存失效操作
/// </summary>
internal static class DictionaryChanges
{
    /// <summary>
    /// 提交后使该类型的快照缓存失效
    /// </summary>
    public static void Invalidate(IAfterCommit afterCommit, string typeCode) =>
        afterCommit.InvalidateTags(ConfigurationCacheKeys.Dictionary(typeCode));
}
