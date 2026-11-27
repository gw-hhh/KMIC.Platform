using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>客户端类型，决定刷新令牌的存储方式及有效期策略。</summary>
public sealed class ClientType : Enumeration<ClientType>
{
    /// <summary>浏览器端：刷新令牌存放在 HttpOnly Cookie，不返回到响应体。</summary>
    public static readonly ClientType Web = new("Web", "网页");

    /// <summary>移动端：刷新令牌随响应返回，客户端须存入系统安全存储。</summary>
    public static readonly ClientType App = new("App", "移动端");

    private ClientType(string code, string name)
        : base(code, name)
    {
    }
}
