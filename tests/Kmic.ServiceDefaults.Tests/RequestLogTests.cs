using Serilog.Events;
using Shouldly;

namespace Kmic.ServiceDefaults.Tests;

public sealed class RequestLogTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task OneSummaryPerRequest_WithRouteTemplate_AndWithoutQueryString()
    {
        await using var app = await TestApp.StartAsync();

        (await app.Client.GetAsync(new Uri("/orders/42?token=abc123&q=secretquery", UriKind.Relative), Ct)).EnsureSuccessStatusCode();
        await app.WaitForRequestSummariesAsync(1);

        var request = app.Logs.Events.Single(e => e.IsRequestSummary());
        request.Level.ShouldBe(LogEventLevel.Information);
        request.Text("RoutePattern").ShouldBe("/orders/{id}");
        request.Text("StatusCode").ShouldBe("200");
        request.Text("RequestMethod").ShouldBe("GET");
        request.Text("RequestPath").ShouldBe("/orders/42");
        request.Properties.Values.Select(LogEventExtensions.Text).ShouldNotContain(v => v.Contains("abc123", StringComparison.Ordinal) || v.Contains("secretquery", StringComparison.Ordinal));
    }

    [Fact]
    public async Task UnmatchedRoute_StillLogged()
    {
        await using var app = await TestApp.StartAsync();

        var response = await app.Client.GetAsync(new Uri("/nothing-here", UriKind.Relative), Ct);
        await app.WaitForRequestSummariesAsync(1);

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        app.Logs.Events.Single(e => e.IsRequestSummary()).Text("RoutePattern").ShouldBe("(unmatched)");
    }

    [Fact]
    public async Task ServerError_LoggedAsError()
    {
        await using var app = await TestApp.StartAsync();

        await app.Client.GetAsync(new Uri("/boom", UriKind.Relative), Ct);
        await app.WaitForRequestSummariesAsync(1);

        app.Logs.Events.Single(e => e.IsRequestSummary()).Level.ShouldBe(LogEventLevel.Error);
    }

    [Fact]
    public async Task UnhandledException_LoggedOnceWithTheException()
    {
        await using var app = await TestApp.StartAsync();

        await Should.ThrowAsync<InvalidOperationException>(() => app.Client.GetAsync(new Uri("/crash", UriKind.Relative), Ct));

        var request = app.Logs.Events.Where(e => e.IsRequestSummary()).ShouldHaveSingleItem();
        request.Level.ShouldBe(LogEventLevel.Error);
        request.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task HealthChecks_NotLogged()
    {
        await using var app = await TestApp.StartAsync();

        (await app.Client.GetAsync(new Uri("/health/live", UriKind.Relative), Ct)).EnsureSuccessStatusCode();

        // 之后再发一个普通请求，它的摘要出现时健康检查的摘要（如果有）早已出现
        await app.Client.GetAsync(new Uri("/orders/sentinel", UriKind.Relative), Ct);
        app.Logs.Events.Where(e => e.IsRequestSummary()).Select(e => e.Text("RequestPath")).ShouldBe(["/orders/sentinel"]);

        app.Logs.Events.Where(e => e.IsRequestSummary()).Select(e => e.Text("RequestPath")).ShouldBe(["/orders/sentinel"]);
    }

    [Fact]
    public async Task Disabled_ByConfiguration()
    {
        await using var app = await TestApp.StartAsync(configuration: new Dictionary<string, string?>(StringComparer.Ordinal) { ["Kmic:Observability:RequestLog"] = "false" });

        (await app.Client.GetAsync(new Uri("/orders/1", UriKind.Relative), Ct)).EnsureSuccessStatusCode();

        app.Logs.Events.ShouldNotContain(e => e.IsRequestSummary());
    }

    [Fact]
    public async Task LongUserAgent_Truncated()
    {
        await using var app = await TestApp.StartAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/orders/1", UriKind.Relative));
        request.Headers.TryAddWithoutValidation("User-Agent", new string('x', 500));

        await app.Client.SendAsync(request, Ct);
        await app.WaitForRequestSummariesAsync(1);

        app.Logs.Events.Single(e => e.IsRequestSummary()).Text("UserAgent")!.Length.ShouldBe(200);
    }
}
