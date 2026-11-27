namespace Kmic.Identity.Presentation.Authentication;

/// <param name="RefreshToken">刷新令牌（移动端必填，Web 端走 Cookie）</param>
public sealed record RefreshRequest(string? RefreshToken)
{
    // 禁用编译器生成的实现，避免输出密码或令牌明文
    public override string ToString() => nameof(RefreshRequest);
}
