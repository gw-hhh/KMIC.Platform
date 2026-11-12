using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.IdentityModel.Tokens;
using Shouldly;

namespace Kmic.BuildingBlocks.Presentation.Tests.Authentication;

/// <summary>
/// 访问令牌的验证、请求的访问上下文加载、授权版本头、当前角色不可用时的拦截
/// </summary>
public sealed class JwtAuthenticationTests : IAsyncLifetime
{
    private const string Protected = "/api/v1/samples/whoami";
    private const string AllowedWhenRoleUnavailable = "/api/v1/samples/whoami-allowed";
    private const string Public = "/api/v1/samples/whoami-public";

    private readonly TokenFactory _tokens = new();
    private readonly TestIdentity _alice = TestIdentity.User("甲").WithRole(r => r.Permissions("Test.Thing.Read"));
    private ApiHost _api = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _tokens.AddKey("2026-11");
        _tokens.AddKey("2026-05");
        _api = await ApiHost.StartAsync(
            [new ListedModule("Samples", AuthEndpoints.All)],
            configuration: _tokens.Configuration("2026-11", validationOnlyKid: "2026-05"),
            jwt: true,
            skipValidation: true);
        _api.Loader.Register(_alice);
    }

    public async ValueTask DisposeAsync()
    {
        await _api.DisposeAsync();
        _tokens.Dispose();
    }

    [Fact]
    public async Task ValidToken_LoadsTheAccessContext_AndSetsTheCurrentIdentity()
    {
        var response = await GetAsync(Protected, _tokens.Create(_alice, "2026-11"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.JsonAsync();
        body.GetProperty("userId").GetString().ShouldBe(_alice.UserId.Value.ToString());
        body.GetProperty("roleId").GetString().ShouldBe(_alice.RoleId.Value.ToString());
        body.GetProperty("sessionId").GetString().ShouldBe(_alice.SessionId.Value.ToString());
        body.GetProperty("origin").GetString().ShouldBe("Http");
        body.GetProperty("isSystem").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task AuthorizationVersion_IsReturnedOnEveryAuthenticatedResponse()
    {
        var token = _tokens.Create(_alice, "2026-11");

        var ok = await GetAsync(Protected, token);
        var notFound = await GetAsync("/api/v1/samples/nothing-here", token);

        ok.Headers.GetValues("X-Authz-Version").ShouldHaveSingleItem().ShouldBe(_alice.AuthorizationVersion.ToString(System.Globalization.CultureInfo.InvariantCulture));
        notFound.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        notFound.Headers.GetValues("X-Authz-Version").ShouldHaveSingleItem().ShouldBe(_alice.AuthorizationVersion.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task AuthorizationVersion_FollowsTheLatestLoad()
    {
        var token = _tokens.Create(_alice, "2026-11");
        _api.Loader.Register(_alice with { AuthorizationVersion = 9 });

        var response = await GetAsync(Protected, token);

        response.Headers.GetValues("X-Authz-Version").ShouldHaveSingleItem().ShouldBe("9");
    }

    [Fact]
    public async Task NoToken_IsAnonymous_AndProtectedEndpointsAnswer401()
    {
        var response = await GetAsync(Protected, token: null);

        await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
        response.Headers.Contains("X-Authz-Version").ShouldBeFalse();
    }

    [Fact]
    public async Task AnonymousEndpoint_SeesAnAnonymousIdentity()
    {
        var response = await GetAsync(Public, token: null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.JsonAsync();
        body.GetProperty("userId").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);
        body.GetProperty("origin").GetString().ShouldBe("Http");
    }

    [Fact]
    public async Task KeyBeingRetired_StillValidatesTokens()
    {
        // 轮换期间新旧密钥并存：旧密钥签的令牌在过期前仍有效
        var response = await GetAsync(Protected, _tokens.Create(_alice, "2026-05"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExpiredToken_Answers401()
    {
        var response = await GetAsync(Protected, _tokens.Create(_alice, "2026-11", expiresIn: TimeSpan.FromMinutes(-5)));

        await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
    }

    [Fact]
    public async Task TokenJustExpired_IsWithinTheClockSkew()
    {
        var response = await GetAsync(Protected, _tokens.Create(_alice, "2026-11", expiresIn: TimeSpan.FromSeconds(-10)));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("other-issuer", TokenFactory.Audience)]
    [InlineData(TokenFactory.Issuer, "other-audience")]
    [InlineData(null, TokenFactory.Audience)]
    [InlineData(TokenFactory.Issuer, null)]
    public async Task WrongIssuerOrAudience_Answers401(string? issuer, string? audience)
    {
        var response = await GetAsync(Protected, _tokens.Create(_alice, "2026-11", issuer, audience));

        await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
    }

    [Fact]
    public async Task TokenSignedByAnUnknownKey_Answers401()
    {
        using var stranger = RSA.Create(2048);

        var response = await GetAsync(Protected, _tokens.Create(_alice, "2026-11", signWith: stranger));

        await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
    }

    [Fact]
    public async Task TokenWithAnUnknownKeyId_Answers401()
    {
        var factory = new TokenFactory();
        try
        {
            factory.AddKey("not-configured");

            var response = await GetAsync(Protected, factory.Create(_alice, "not-configured"));

            await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
        }
        finally
        {
            factory.Dispose();
        }
    }

    [Fact]
    public async Task SymmetricAlgorithm_IsRejected()
    {
        var response = await GetAsync(Protected, TokenFactory.CreateSymmetric(_alice));

        await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
    }

    [Fact]
    public async Task UnsignedToken_IsRejected()
    {
        // alg 为 none 的令牌：头、载荷、空签名
        var header = Base64Url("""{"alg":"none","typ":"JWT"}""");
        var payload = Base64Url($$"""{"iss":"{{TokenFactory.Issuer}}","aud":"{{TokenFactory.Audience}}","sub":"{{_alice.UserId.Value}}","sid":"{{_alice.SessionId.Value}}","rid":"{{_alice.RoleId.Value}}","exp":4102444800}""");

        var response = await GetAsync(Protected, $"{header}.{payload}.");

        await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task TokenWithoutSessionOrRole_IsTreatedAsNotLoggedIn(bool includeSession, bool includeRole)
    {
        var token = _tokens.Create(_alice, "2026-11", includeSession: includeSession, includeRole: includeRole);

        var protectedResponse = await GetAsync(Protected, token);
        var publicResponse = await GetAsync(Public, token);

        await protectedResponse.ShouldBeProblemAsync(401, "Common.Unauthenticated");
        publicResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await publicResponse.JsonAsync()).GetProperty("userId").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);
    }

    [Fact]
    public async Task InvalidSession_IsTreatedAsNotLoggedIn_NotAsAnError()
    {
        // 会话已被撤销或用户已禁用：受保护的端点 401，匿名端点（登录、刷新）照常可用
        var token = _tokens.Create(_alice, "2026-11");
        _api.Loader.Remove(_alice);

        var protectedResponse = await GetAsync(Protected, token);
        var publicResponse = await GetAsync(Public, token);

        await protectedResponse.ShouldBeProblemAsync(401, "Common.Unauthenticated");
        publicResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await publicResponse.JsonAsync()).GetProperty("userId").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);
    }

    [Fact]
    public async Task SessionOfAnotherToken_IsNotAccepted()
    {
        var other = _alice with { SessionId = Kmic.BuildingBlocks.Domain.Identifiers.SessionId.New() };

        var response = await GetAsync(Protected, _tokens.Create(other, "2026-11"));

        await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
    }

    [Fact]
    public async Task RoleRevoked_ProtectedEndpointsAnswer403CurrentRoleUnavailable()
    {
        var token = _tokens.Create(_alice, "2026-11");
        _api.Loader.RevokeRole(_alice);

        var response = await GetAsync(Protected, token);

        var body = await response.ShouldBeProblemAsync(403, "Identity.CurrentRoleUnavailable");
        body.GetProperty("retryable").GetBoolean().ShouldBeFalse();

        // 授权版本照常返回：前端据此重新拉取自己的信息
        response.Headers.Contains("X-Authz-Version").ShouldBeTrue();
    }

    [Fact]
    public async Task RoleRevoked_DeclaredEndpointsStillWork_WithoutRolePrivileges()
    {
        var token = _tokens.Create(_alice, "2026-11");
        _api.Loader.RevokeRole(_alice);

        var response = await GetAsync(AllowedWhenRoleUnavailable, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.JsonAsync()).GetProperty("userId").GetString().ShouldBe(_alice.UserId.Value.ToString());
    }

    [Fact]
    public async Task RoleRevoked_AnonymousEndpointsAreNotAffected()
    {
        var token = _tokens.Create(_alice, "2026-11");
        _api.Loader.RevokeRole(_alice);

        var response = await GetAsync(Public, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RoleRestored_WorksAgain()
    {
        var token = _tokens.Create(_alice, "2026-11");
        _api.Loader.RevokeRole(_alice);
        (await GetAsync(Protected, token)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        _api.Loader.Register(_alice);

        (await GetAsync(Protected, token)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static string Base64Url(string json) => Base64UrlEncoder.Encode(json);

    private async Task<HttpResponseMessage> GetAsync(string path, string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _api.Anonymous.SendAsync(request, Ct);
    }
}
