using System.Diagnostics.CodeAnalysis;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 接口客户端类型编码，使用小写 web 或 app
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
    /// 尝试解析客户端类型编码
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
    /// 解析已校验的客户端类型编码；未知编码表示调用缺陷并抛异常
    /// </summary>
    public static ClientType Parse(string text) =>
        TryParse(text, out var clientType) ? clientType : throw new ArgumentException($"未知客户端类型：{text}。", nameof(text));

    /// <summary>
    /// 访问令牌中 cty 声明的编码
    /// </summary>
    public static string Claim(ClientType clientType)
    {
        ArgumentNullException.ThrowIfNull(clientType);
        return clientType == ClientType.Web ? Web : App;
    }
}
