using Spike02.Domain;

namespace Spike02.Infrastructure;

/// <summary>
/// 当前请求的数据范围（05-05 第 5.3 节）；原型中把当前用户也放在这里
/// </summary>
public interface IDataScope
{
    bool Unrestricted { get; }

    UserId? CurrentUser { get; }

    EffectiveScope For(string scopeObject);
}
