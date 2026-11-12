using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 客户端类型，决定刷新令牌怎样存放、滑动与绝对有效期多长
/// </summary>
public sealed class ClientType : Enumeration<ClientType>
{
    /// <summary>
    /// 浏览器：刷新令牌放在 HttpOnly Cookie 里，不出现在响应体
    /// </summary>
    public static readonly ClientType Web = new("Web", "网页");

    /// <summary>
    /// 移动端：刷新令牌在响应体里返回，由 App 存进系统安全存储
    /// </summary>
    public static readonly ClientType App = new("App", "移动端");

    private ClientType(string code, string name)
        : base(code, name)
    {
    }
}
