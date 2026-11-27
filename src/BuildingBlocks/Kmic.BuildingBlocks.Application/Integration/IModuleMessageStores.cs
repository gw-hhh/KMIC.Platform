namespace Kmic.BuildingBlocks.Application.Integration;

public interface IModuleMessageStores
{
    /// <summary>所有已注册数据库模块的消息存储列表，按模块名排序。</summary>
    IReadOnlyList<IModuleMessageStore> All { get; }

    /// <summary>按模块名获取消息存储。</summary>
    IModuleMessageStore? Find(string module);

    /// <summary>获取待终结的订阅名称集合。</summary>
    Task<IReadOnlySet<string>> GetRetiringSubscriptionsAsync(CancellationToken ct);
}
