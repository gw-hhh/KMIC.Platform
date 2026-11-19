using System.Net;
using System.Net.Http.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.Api.Tests.Health;

/// <summary>
/// 入站防护：转发头只信任可信代理、安全响应头、请求体上限
/// </summary>
/// <remarks>
/// 测试服务器没有真实的来源地址，用 X-Test-Peer 头模拟直连的对端
/// </remarks>
public sealed class InboundProtectionTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IAsyncLifetime, IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string TrustedPeer = "10.0.0.5";
    private const string UntrustedPeer = "198.51.100.7";
    private const string RealClient = "203.0.113.9";

    private ApiFactory _factory = null!;
    private IdentitySeed _seed = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        var connection = template.RuntimeConnectionString(database.Name);
        _factory = database.Shared(() => new ApiFactory(
            connection,
            realIdentity: true,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["Kmic:Http:TrustedProxies:0"] = "10.0.0.0/16" },
            services => services.AddTransient<IStartupFilter, PeerFilter>()));
        _seed = new IdentitySeed(connection);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _seed.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ForwardedFor_FromATrustedProxy_BecomesTheClientAddress()
    {
        await FailedLoginAsync("trusted-proxy", TrustedPeer, forwardedFor: RealClient);

        (await LastLoginIpAsync("trusted-proxy")).ShouldBe(RealClient);
    }

    [Fact]
    public async Task ForwardedFor_FromAnUntrustedPeer_IsIgnored()
    {
        await FailedLoginAsync("untrusted-peer", UntrustedPeer, forwardedFor: "192.0.2.66");

        (await LastLoginIpAsync("untrusted-peer")).ShouldBe(UntrustedPeer);
    }

    [Theory]
    [InlineData(RealClient)]
    [InlineData("192.168.1.20")]
    public async Task DependencyEndpoint_StaysHidden_ForARequestThatCameThroughTheTrustedProxy(string clientAddress)
    {
        using var client = _factory.CreateClient();
        using var request = Get("/health/dependencies", TrustedPeer, forwardedFor: clientAddress);

        var response = await client.SendAsync(request, Ct);

        // 转发头中间件处理完会清掉 X-Forwarded-For，并把来源换成客户端地址：客户端是内网地址（如经 VPN 走内部负载均衡）时，
        // 不能因此把经过代理的请求当成内部请求
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DependencyEndpoint_IsReachable_ForADirectInternalRequest()
    {
        using var client = _factory.CreateClient();
        using var request = Get("/health/dependencies", TrustedPeer);

        var response = await client.SendAsync(request, Ct);

        response.StatusCode.ShouldNotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EveryResponse_CarriesTheSecurityHeaders_AndApiResponsesAreNotCached()
    {
        using var client = _factory.CreateClient();

        var api = await client.SendAsync(Get("/api/v1/identity/me", UntrustedPeer), Ct);
        var other = await client.SendAsync(Get("/health/live", UntrustedPeer), Ct);

        foreach (var response in new[] { api, other })
        {
            response.Headers.GetValues("X-Content-Type-Options").ShouldHaveSingleItem().ShouldBe("nosniff");
            response.Headers.GetValues("X-Frame-Options").ShouldHaveSingleItem().ShouldBe("DENY");
            response.Headers.GetValues("Referrer-Policy").ShouldHaveSingleItem().ShouldBe("no-referrer");
            response.Headers.Contains("Server").ShouldBeFalse();
        }

        api.StatusCode.ShouldBe(HttpStatusCode.Unauthorized, "没带令牌的接口请求，错误响应同样带安全头");
        api.Headers.CacheControl!.NoStore.ShouldBeTrue();
    }

    [Fact]
    public async Task Hsts_IsSentOnlyWhenTheUsersConnectionWasHttps()
    {
        using var client = _factory.CreateClient();

        using var secure = Get("/health/live", TrustedPeer, forwardedFor: RealClient);
        secure.Headers.Add("X-Forwarded-Proto", "https");
        var viaHttps = await client.SendAsync(secure, Ct);
        var plain = await client.SendAsync(Get("/health/live", TrustedPeer, forwardedFor: RealClient), Ct);

        viaHttps.Headers.GetValues("Strict-Transport-Security").ShouldHaveSingleItem().ShouldBe("max-age=31536000");
        plain.Headers.Contains("Strict-Transport-Security").ShouldBeFalse();
    }

    [Fact]
    public void RequestBodies_AreLimitedToOneMegabyteByDefault_AndTheServerHeaderIsOff()
    {
        var kestrel = _factory.Services.GetRequiredService<IOptions<KestrelServerOptions>>().Value;

        kestrel.Limits.MaxRequestBodySize.ShouldBe(1_048_576);
        kestrel.AddServerHeader.ShouldBeFalse();
    }

    private static HttpRequestMessage Get(string path, string peer, string? forwardedFor = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add(PeerFilter.Header, peer);
        if (forwardedFor is not null)
        {
            request.Headers.Add("X-Forwarded-For", forwardedFor);
        }

        return request;
    }

    private async Task FailedLoginAsync(string account, string peer, string forwardedFor)
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/identity/auth/login")
        {
            Content = JsonContent.Create(new { account, password = "wrong password here", clientType = "app" }),
        };
        request.Headers.Add(PeerFilter.Header, peer);
        request.Headers.Add("X-Forwarded-For", forwardedFor);

        var response = await client.SendAsync(request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // 安全事件由后台批量写库，要等一下
    private async Task<string?> LastLoginIpAsync(string account) =>
        await Eventually.UntilAsync(
            async _ => await _seed.ScalarAsync<string>(
                """
                SELECT "Details"->>'ip' FROM "Identity"."SecurityEvent"
                WHERE "Type" = 'LoginFailed' AND "AttemptedAccount" = @account
                ORDER BY "OccurredAt" DESC LIMIT 1
                """,
                ("account", account)),
            ip => ip is not null,
            $"{account} 的登录失败事件写入数据库",
            ct: Ct);

    /// <summary>
    /// 测试服务器没有对端地址：由请求头指定，放在管道最前面
    /// </summary>
    private sealed class PeerFilter : IStartupFilter
    {
        public const string Header = "X-Test-Peer";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    if (context.Request.Headers.TryGetValue(Header, out var peer))
                    {
                        context.Connection.RemoteIpAddress = IPAddress.Parse(peer.ToString());
                    }

                    await nextMiddleware(context);
                });
                next(app);
            };
    }
}
