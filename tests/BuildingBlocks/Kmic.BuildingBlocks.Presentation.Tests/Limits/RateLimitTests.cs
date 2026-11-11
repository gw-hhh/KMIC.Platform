using System.Net;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Limits;

/// <summary>
/// 按时间限流：已登录的按用户，没有登录的按 IP，各有各的限额
/// </summary>
public sealed class RateLimitTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Anonymous_IsLimitedPerIp_With429()
    {
        await using var api = await StartAsync(anonymous: 2, authenticated: 100);

        var statuses = new List<HttpStatusCode>();
        HttpResponseMessage? last = null;
        for (var i = 0; i < 3; i++)
        {
            last = await api.Anonymous.GetAsync("/api/v1/samples/me", Ct);
            statuses.Add(last.StatusCode);
        }

        // 没有登录的请求前两次被认证拦下（401），也都算进限额
        statuses.ShouldBe([HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.TooManyRequests]);
        var body = await last!.ShouldBeProblemAsync(429, "Common.TooManyRequests");
        body.GetProperty("retryable").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task Rejection_CarriesRetryAfterInSeconds()
    {
        await using var api = await StartAsync(anonymous: 1, authenticated: 100);
        await api.Anonymous.GetAsync("/api/v1/samples/me", Ct);

        var response = await api.Anonymous.GetAsync("/api/v1/samples/me", Ct);

        var seconds = int.Parse(response.Headers.GetValues("Retry-After").ShouldHaveSingleItem(), System.Globalization.CultureInfo.InvariantCulture);
        seconds.ShouldBeInRange(1, 60);
    }

    [Fact]
    public async Task Authenticated_IsLimitedPerUser()
    {
        await using var api = await StartAsync(anonymous: 100, authenticated: 2);
        using var bob = api.ClientFor("bob");

        var alice = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            alice.Add((await api.Client.GetAsync("/api/v1/samples/me", Ct)).StatusCode);
        }

        alice.ShouldBe([HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.TooManyRequests]);

        // 另一个用户有自己的额度
        (await bob.GetAsync("/api/v1/samples/me", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AnonymousAndAuthenticated_HaveSeparateLimits()
    {
        await using var api = await StartAsync(anonymous: 1, authenticated: 5);

        await api.Anonymous.GetAsync("/api/v1/samples/me", Ct);
        var anonymousAgain = await api.Anonymous.GetAsync("/api/v1/samples/me", Ct);
        var loggedIn = await api.Client.GetAsync("/api/v1/samples/me", Ct);

        anonymousAgain.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        loggedIn.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DefaultLimits_AreGenerousForNormalUse()
    {
        await using var api = await ApiHost.StartAsync([ThingEndpoints.Module()]);
        api.Script.On<MeQuery, string>((_, _) => "alice");

        for (var i = 0; i < 20; i++)
        {
            (await api.Client.GetAsync("/api/v1/samples/me", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    [Theory]
    [InlineData("Kmic:Http:HeavyConcurrency", "0")]
    [InlineData("Kmic:Http:AnonymousRequestsPerMinute", "0")]
    [InlineData("Kmic:Http:AuthenticatedRequestsPerMinute", "-1")]
    [InlineData("Kmic:Http:RequestTimeout", "00:00:00")]
    public async Task InvalidConfiguration_RefusesToStart(string key, string value)
    {
        var host = ApiHost.Create([ThingEndpoints.Module()], configuration: new Dictionary<string, string?>(StringComparer.Ordinal) { [key] = value });
        await using var _ = host;

        await Should.ThrowAsync<Microsoft.Extensions.Options.OptionsValidationException>(host.StartAsync());
    }

    private static Task<ApiHost> StartAsync(int anonymous, int authenticated)
    {
        var configuration = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Kmic:Http:AnonymousRequestsPerMinute"] = anonymous.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["Kmic:Http:AuthenticatedRequestsPerMinute"] = authenticated.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        return StartCoreAsync(configuration);
    }

    private static async Task<ApiHost> StartCoreAsync(Dictionary<string, string?> configuration)
    {
        var api = await ApiHost.StartAsync([ThingEndpoints.Module()], configuration: configuration);
        api.Script.On<MeQuery, string>((_, _) => "alice");
        return api;
    }
}
