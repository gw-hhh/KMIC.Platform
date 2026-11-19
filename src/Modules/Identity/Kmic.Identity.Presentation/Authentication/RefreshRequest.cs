namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 刷新令牌请求；移动端提交请求体，网页客户端使用 Cookie
/// </summary>
/// <param name="RefreshToken">刷新令牌</param>
public sealed record RefreshRequest(string? RefreshToken)
{
    // 不用编译器生成的：它会把密码、令牌的值也打出来
    public override string ToString() => nameof(RefreshRequest);
}
