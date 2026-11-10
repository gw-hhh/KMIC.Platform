namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 当前请求的数据范围；完全在内存中计算，请求内按范围对象缓存，不跨请求
/// </summary>
public interface IDataScope
{
    /// <summary>
    /// 系统身份不受数据范围限制
    /// </summary>
    bool Unrestricted { get; }

    /// <summary>
    /// 某个范围对象上的有效范围
    /// </summary>
    EffectiveScope For(string scopeObject);
}
