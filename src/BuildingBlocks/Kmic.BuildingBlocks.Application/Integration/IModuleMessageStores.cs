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
    /// <param name="module">模块名</param>
    IModuleMessageStore? Find(string module);

    /// <summary>
    /// 处于“待终结”的订阅：订阅已经从代码里去掉，消息照常投递并积压，等运维确认终结
    /// </summary>
    /// <remarks>
    /// 它们的积压是预期内的，不计入通用的积压告警，管理视图单独列出
    /// </remarks>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlySet<string>> GetRetiringSubscriptionsAsync(CancellationToken ct);
}
