using System.Diagnostics.CodeAnalysis;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 客户端类型在接口上的写法：小写的 web、app
/// </summary>
public static class ClientKinds
{
    /// <summary>
    /// 浏览器
    /// </summary>
    public const string Web = "web";

    /// <summary>
    /// 移动端
    /// </summary>
    public const string App = "app";

    /// <summary>
    /// 解析接口上的写法
    /// </summary>
    public static bool TryParse(string? text, [NotNullWhen(true)] out ClientType? clientType)
    {
        clientType = text switch
        {
            Web => ClientType.Web,
            App => ClientType.App,
            _ => null,
        };
        return clientType is not null;
    }

    /// <summary>
    /// 解析接口上的写法，不认识时抛异常；请求已经校验过，走到这里不认识说明是程序缺陷
    /// </summary>
    public static ClientType Parse(string text) =>
        TryParse(text, out var clientType) ? clientType : throw new ArgumentException($"不认识的客户端类型：{text}。", nameof(text));

    /// <summary>
    /// 令牌里 cty 声明的写法
    /// </summary>
    public static string Claim(ClientType clientType)
    {
        ArgumentNullException.ThrowIfNull(clientType);
        return clientType == ClientType.Web ? Web : App;
    }
}
