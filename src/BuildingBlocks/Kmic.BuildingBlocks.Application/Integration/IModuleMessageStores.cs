namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 全部模块的消息存储
/// </summary>
public interface IModuleMessageStores
{
    /// <summary>
    /// 每个注册了数据库的模块一个，按模块名排好
    /// </summary>
    IReadOnlyList<IModuleMessageStore> All { get; }

    /// <summary>
    /// 按模块名取，没有这个模块时返回空
    /// </summary>
    IModuleMessageStore? Find(string module);

    /// <summary>
    /// 待终结的订阅
    /// </summary>
    Task<IReadOnlySet<string>> GetRetiringSubscriptionsAsync(CancellationToken ct);
}
