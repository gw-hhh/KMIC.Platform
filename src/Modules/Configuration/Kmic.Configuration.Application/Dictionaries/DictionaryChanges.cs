using Kmic.BuildingBlocks.Application.Events;

namespace Kmic.Configuration.Application.Dictionaries;

internal static class DictionaryChanges
{
    public static void Invalidate(IAfterCommit afterCommit, string typeCode) =>
        afterCommit.InvalidateTags(ConfigurationCacheKeys.Dictionary(typeCode));
}
