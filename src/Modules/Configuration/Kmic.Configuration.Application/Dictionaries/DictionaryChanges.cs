using Kmic.BuildingBlocks.Application.Events;

namespace Kmic.Configuration.Application.Dictionaries;

/// <summary>
/// 字典变更命令的收尾
/// </summary>
internal static class DictionaryChanges
{
    /// <summary>
    /// 提交之后失效这个类型的快照缓存：提交前清掉的话，并发的请求会用旧数据把缓存重新填上
    /// </summary>
    public static void Invalidate(IAfterCommit afterCommit, string typeCode) =>
        afterCommit.InvalidateTags(ConfigurationCacheKeys.Dictionary(typeCode));
}
