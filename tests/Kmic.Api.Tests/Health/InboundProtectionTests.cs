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

// 测试服务器无真实对端 IP，通过 X-Test-Peer 头模拟直连对端地址
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

        // 转发头中间件处理后会清除 X-Forwarded-For 并将来源地址更新为客户端 IP：当客户端位于内网（如经 VPN 访问内部负载均衡）时，
        // 不应将经代理转发的请求误判为内部直连请求
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

        api.StatusCode.ShouldBe(HttpStatusCode.Unauthorized, "未携带令牌的接口请求在返回 401 错误响应时也应包含安全响应头");
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

    // 安全事件由后台服务批量写入数据库，需轮询等待落库完成
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

    // 在请求管线最前端读取 X-Test-Peer 头模拟对端 IP
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
