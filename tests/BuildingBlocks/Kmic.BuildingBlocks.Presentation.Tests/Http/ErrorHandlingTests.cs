using System.Net;
using System.Net.Http.Headers;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Http;

/// <summary>
/// 不经过 Result、由中间件或框架产生的错误，格式与业务错误一致
/// </summary>
public sealed class ErrorHandlingTests : IAsyncLifetime
{
    private ApiHost _api = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => _api = await ApiHost.StartAsync([ThingEndpoints.Module()]);

    public async ValueTask DisposeAsync() => await _api.DisposeAsync();

    [Fact]
    public async Task NotLoggedIn_Returns401()
    {
        var response = await _api.Anonymous.GetAsync("/api/v1/samples/me", Ct);

        var body = await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
        body.GetProperty("retryable").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task UnknownRoute_Returns404()
    {
        var response = await _api.Client.GetAsync("/api/v1/samples/nothing-here", Ct);

        await response.ShouldBeProblemAsync(404, "Common.RouteNotFound");
    }

    [Fact]
    public async Task WrongMethod_Returns405()
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/samples/things/{ThingId.New().Value}");

        var response = await _api.Client.SendAsync(request, Ct);

        await response.ShouldBeProblemAsync(405, "Common.MethodNotAllowed");
    }

    [Fact]
    public async Task UnhandledException_Returns500_WithoutLeakingDetails()
    {
        var response = await _api.Client.GetAsync("/api/v1/samples/boom", Ct);

        var body = await response.ShouldBeProblemAsync(500, "Common.InternalError");
        body.GetProperty("retryable").GetBoolean().ShouldBeFalse();
        var text = await response.Content.ReadAsStringAsync(Ct);

        // 异常消息、类型名、堆栈都不出现在响应里
        text.ShouldNotContain("secret");
        text.ShouldNotContain("InvalidOperationException");
        text.ShouldNotContain("   at ");
        body.TryGetProperty("detail", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task UnhandledException_StillCarriesTheTraceIdHeader()
    {
        var response = await _api.Client.GetAsync("/api/v1/samples/boom", Ct);

        // 异常处理会清掉已设置的响应头，X-Trace-Id 要在响应真正开始时才写
        response.Headers.GetValues("X-Trace-Id").ShouldHaveSingleItem().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SuccessfulResponse_CarriesTheTraceIdHeader()
    {
        _api.Script.On<MeQuery, string>((_, _) => "alice");

        var response = await _api.Client.GetAsync("/api/v1/samples/me", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.GetValues("X-Trace-Id").ShouldHaveSingleItem().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RequestBodyTooLarge_Returns413()
    {
        var response = await _api.Client.GetAsync("/api/v1/samples/too-large", Ct);

        await response.ShouldBeProblemAsync(413, "Common.PayloadTooLarge");
    }

    [Fact]
    public async Task NonJsonContent_Returns415()
    {
        using var content = new StringContent("name=a", new MediaTypeHeaderValue("text/plain"));

        var response = await _api.Client.PostAsync("/api/v1/samples/things", content, Ct);

        await response.ShouldBeProblemAsync(415, "Common.UnsupportedMediaType");
    }

    [Fact]
    public async Task MalformedRouteParameter_Returns400()
    {
        var response = await _api.Client.GetAsync("/api/v1/samples/things/not-a-uuid", Ct);

        var body = await response.ShouldBeProblemAsync(400, "Common.MalformedRequest");
        body.TryGetProperty("errors", out _).ShouldBeFalse();
    }

    [Fact]
    public void UnlistedStatusCode_FallsBackToAGenericCode()
    {
        var problem = FrameworkProblems.ForStatus(418);

        problem.Status.ShouldBe(418);
        problem.Code.ShouldBe("Common.HttpError");
        problem.Retryable.ShouldBeFalse();
    }

    [Fact]
    public void GatewayStatuses_AreRetryable()
    {
        FrameworkProblems.ForStatus(502).Retryable.ShouldBeTrue();
        FrameworkProblems.ForStatus(503).Retryable.ShouldBeTrue();
    }

    [Fact]
    public void ErrorCatalog_HasUniqueWellFormedCodes()
    {
        var codes = FrameworkProblems.All.Select(p => p.Code).ToList();

        codes.Distinct(StringComparer.Ordinal).Count().ShouldBe(codes.Count);
        codes.ShouldAllBe(code => code.Split('.').Length == 2 && code.Split('.').All(part => part.Length > 0 && char.IsUpper(part[0])));
    }

    [Fact]
    public void RateLimitCodes_AreRetryable()
    {
        FrameworkProblems.TooManyRequests.Retryable.ShouldBeTrue();
        FrameworkProblems.TooBusy.Retryable.ShouldBeTrue();
        FrameworkProblems.TooManyRequests.Status.ShouldBe(429);
        FrameworkProblems.TooBusy.Status.ShouldBe(429);
    }
}
