namespace Kmic.Identity.Presentation.Authentication;

/// <param name="Account">登录名</param>
/// <param name="Password">密码</param>
/// <param name="ClientType">客户端类型</param>
public sealed record LoginRequest(string Account, string Password, string ClientType)
{
    // 禁用编译器生成的实现，避免输出密码或令牌明文
    public override string ToString() => nameof(LoginRequest);
}
