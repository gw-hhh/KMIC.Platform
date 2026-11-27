namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>当前请求的数据权限。</summary>
public interface IDataScope
{
    /// <summary>是否不受数据权限限制。</summary>
    bool Unrestricted { get; }

    /// <summary>获取指定范围对象的有效数据范围。</summary>
    EffectiveScope For(string scopeObject);
}
