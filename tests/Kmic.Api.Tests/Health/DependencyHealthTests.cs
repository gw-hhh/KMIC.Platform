using System.Net;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Hosting.Health;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Observability;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.Api.Tests.Health;

/// <summary>
/// 依赖状态端点，以及框架配置项能不能从配置里读到
/// </summary>
public sealed class DependencyHealthTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Path = "/health/dependencies";

    [Fact]
    public async Task Dependencies_ListEveryCheckWithItsStatus()
    {
        using var client = ClientFor();

        var report = await Eventually.UntilAsync(
            async ct => await JsonAsync(await client.GetAsync(Path, ct)),
            r => Is(r.GetProperty("status"), "Healthy"),
            "监听连接建立后整体健康",
            ct: Ct);

        var entries = report.GetProperty("entries").EnumerateArray().ToDictionary(e => e.GetProperty("name").GetString()!, e => e, StringComparer.Ordinal);
        entries.Keys.ShouldBe(["database", "file-storage", "message-listener"], ignoreOrder: true);
        entries["database"].GetProperty("status").GetString().ShouldBe("Healthy");
        entries["message-listener"].GetProperty("description").GetString().ShouldBe("监听连接正常");
        entries["file-storage"].GetProperty("description").GetString().ShouldBe("没有启用文件存储");
        report.GetProperty("totalDurationMs").GetDouble().ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("X-Forwarded-For", "203.0.113.9")]
    [InlineData("Forwarded", "for=203.0.113.9")]
    [InlineData("X-Real-IP", "203.0.113.9")]
    public async Task RequestThroughAProxy_IsNotFound(string header, string value)
    {
        using var client = ClientFor();
        using var request = new HttpRequestMessage(HttpMethod.Get, Path);
        request.Headers.Add(header, value);

        var response = await client.SendAsync(request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync(Ct);
        body.ShouldNotContain("database");
        body.ShouldNotContain("entries");
    }

    [Fact]
    public async Task FailingCheck_ReportsUnhealthyWithoutExceptionDetails()
    {
        await using var factory = WithChecks(checks => checks
            .AddCheck("broken", () => throw new InvalidOperationException("password=hunter2 host=10.0.0.5"), tags: [DependencyHealth.Tag]));
        using var client = factory.CreateClient();

        var response = await client.GetAsync(Path, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        var text = await response.Content.ReadAsStringAsync(Ct);
        text.ShouldNotContain("hunter2");
        text.ShouldNotContain("10.0.0.5");
        var report = await JsonAsync(response);
        report.GetProperty("status").GetString().ShouldBe("Unhealthy");
        var broken = report.GetProperty("entries").EnumerateArray().Single(e => Is(e.GetProperty("name"), "broken"));
        broken.GetProperty("status").GetString().ShouldBe("Unhealthy");
        broken.GetProperty("description").GetString().ShouldBe("检查失败");
    }

    [Fact]
    public async Task DegradedCheck_StillAnswers200()
    {
        await using var factory = WithChecks(checks => checks
            .AddCheck("limping", () => HealthCheckResult.Degraded("备用线路"), tags: [DependencyHealth.Tag]));
        using var client = factory.CreateClient();

        var response = await client.GetAsync(Path, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(response)).GetProperty("status").GetString().ShouldBe("Degraded");
    }

    [Fact]
    public async Task SlowCheck_ReportsTimeoutAndDoesNotHoldBackTheOthers()
    {
        await using var factory = WithChecks(checks => checks.AddAsyncCheck(
            "hanging",
            async ct =>
            {
                await new TaskCompletionSource().Task.WaitAsync(ct);
                return HealthCheckResult.Healthy();
            },
            tags: [DependencyHealth.Tag],
            timeout: TimeSpan.FromMilliseconds(300)));
        using var client = factory.CreateClient();

        var report = await JsonAsync(await client.GetAsync(Path, Ct));

        var entries = report.GetProperty("entries").EnumerateArray().ToDictionary(e => e.GetProperty("name").GetString()!, e => e, StringComparer.Ordinal);
        entries["hanging"].GetProperty("status").GetString().ShouldBe("Unhealthy");
        entries["hanging"].GetProperty("description").GetString().ShouldBe("超时");
        entries["database"].GetProperty("status").GetString().ShouldBe("Healthy");
    }

    [Fact]
    public async Task ReadyAndLive_DoNotListDependencies()
    {
        await using var factory = WithChecks(checks => checks
            .AddCheck("broken", () => HealthCheckResult.Unhealthy("坏了"), tags: [DependencyHealth.Tag]));
        using var client = factory.CreateClient();

        var ready = await client.GetAsync("/health/ready", Ct);
        var live = await client.GetAsync("/health/live", Ct);

        ready.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ready.Content.ReadAsStringAsync(Ct)).ShouldBe("Healthy");
        (await live.Content.ReadAsStringAsync(Ct)).ShouldBe("Healthy");
    }

    [Fact]
    public void FrameworkOptions_ReadFromConfiguration()
    {
        using var factory = Factory.WithWebHostBuilder(builder => builder
            .UseSetting("Kmic:Jobs:Parallelism", "7")
            .UseSetting("Kmic:Messaging:PollInterval", "00:00:09")
            .UseSetting("Kmic:Pipeline:SlowMessageThreshold", "00:00:03")
            .UseSetting("Kmic:Observability:SlowQueryMs", "123")
            .UseSetting("Kmic:Observability:RecordSqlText", "false")
            .UseSetting("Kmic:BusinessTimeZone", "Asia/Tokyo"));

        factory.Services.GetRequiredService<IOptions<JobOptions>>().Value.Parallelism.ShouldBe(7);
        factory.Services.GetRequiredService<IOptions<MessagingOptions>>().Value.PollInterval.ShouldBe(TimeSpan.FromSeconds(9));
        factory.Services.GetRequiredService<IOptions<PipelineOptions>>().Value.SlowMessageThreshold.ShouldBe(TimeSpan.FromSeconds(3));
        var diagnostics = factory.Services.GetRequiredService<IOptions<DatabaseDiagnosticsOptions>>().Value;
        diagnostics.SlowQueryMs.ShouldBe(123);
        diagnostics.RecordSqlText.ShouldBeFalse();
        factory.Services.GetRequiredService<Kmic.BuildingBlocks.Application.Time.IBusinessCalendar>().Zone.Id.ShouldBe("Asia/Tokyo");
    }

    [Fact]
    public void InvalidDiagnosticsOptions_RefuseToStart()
    {
        using var factory = Factory.WithWebHostBuilder(builder => builder
            .UseSetting("Kmic:Observability:SlowQueryMs", "0")
            .UseSetting("Kmic:Observability:BacklogSampleInterval", "00:00:01"));

        Action start = () => factory.CreateClient().Dispose();

        var exception = Should.Throw<OptionsValidationException>(start);

        exception.Failures.Count().ShouldBe(2);
    }

    private static bool Is(JsonElement value, string expected) => string.Equals(value.GetString(), expected, StringComparison.Ordinal);

    private WebApplicationFactory<Program> WithChecks(Action<IHealthChecksBuilder> add) => Factory.WithWebHostBuilder(builder =>
        builder.ConfigureTestServices(services => add(services.AddHealthChecks())));
}
