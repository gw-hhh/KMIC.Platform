using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Shouldly;

namespace Kmic.ServiceDefaults.Tests;

public sealed class TelemetryTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task FrameworkActivitySources_Exported_OthersNot()
    {
        await using var app = await TestApp.StartAsync();
        using var kmic = new ActivitySource("Kmic.Messaging");
        using var other = new ActivitySource("Somebody.Else");

        using (kmic.StartActivity("handle"))
        {
        }

        using (other.StartActivity("unrelated"))
        {
        }

        app.ForceFlush();
        HasSpan(app, "handle").ShouldBeTrue();
        HasSpan(app, "unrelated").ShouldBeFalse();
    }

    [Fact]
    public async Task HttpRequest_GetsASpan_HealthCheckDoesNot()
    {
        await using var app = await TestApp.StartAsync();

        await app.Client.GetAsync(new Uri("/orders/7", UriKind.Relative), Ct);
        await app.Client.GetAsync(new Uri("/health/live", UriKind.Relative), Ct);

        app.ForceFlush();
        var paths = app.Spans.Select(s => s.GetTagItem("url.path")?.ToString()).ToList();
        paths.Any(p => string.Equals(p, "/orders/7", StringComparison.Ordinal)).ShouldBeTrue();
        paths.Any(p => string.Equals(p, "/health/live", StringComparison.Ordinal)).ShouldBeFalse();
    }

    [Fact]
    public async Task NoSampling_EveryRootSpanRecorded()
    {
        await using var app = await TestApp.StartAsync();
        using var source = new ActivitySource("Kmic.Messaging");

        for (var i = 0; i < 50; i++)
        {
            using var activity = source.StartActivity($"root-{i}");
            activity.ShouldNotBeNull();
            activity.Recorded.ShouldBeTrue();
        }

        app.ForceFlush();
        app.Spans.Count(s => s.DisplayName.StartsWith("root-", StringComparison.Ordinal)).ShouldBe(50);
    }

    [Fact]
    public async Task FrameworkMeters_Exported()
    {
        await using var app = await TestApp.StartAsync();
        var counter = TestApp.NewCounter("Kmic.Framework", "kmic.test.counter");

        counter.Add(3);

        app.ForceFlush();
        app.Metrics.Any(m => string.Equals(m.Name, "kmic.test.counter", StringComparison.Ordinal)).ShouldBeTrue();
    }

    [Fact]
    public async Task ResourceCarriesTheServiceName()
    {
        await using var app = await TestApp.StartAsync(configuration: new Dictionary<string, string?>(StringComparer.Ordinal) { ["OTEL_SERVICE_NAME"] = "kmic-api" });

        var attributes = app.Services.GetRequiredService<TracerProvider>().GetResource().Attributes.ToDictionary(a => a.Key, a => a.Value, StringComparer.Ordinal);

        attributes["service.name"].ShouldBe("kmic-api");
        attributes.ShouldContainKey("service.instance.id");
        attributes.ShouldContainKey("service.version");
    }

    private static bool HasSpan(TestApp app, string name) => app.Spans.Any(s => string.Equals(s.DisplayName, name, StringComparison.Ordinal));
}
