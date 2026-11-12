using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Kmic.ServiceDefaults.Tests;

[SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "测试里直接调用 ILogger，模板是固定的字面量")]
[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "测试里直接调用 ILogger，不用源码生成的日志方法")]
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "测试里的参数都是现成的字面量")]
public sealed class LoggingDefaultsTests
{
    [Fact]
    public async Task EveryEvent_CarriesServiceInstanceRolesAndVersion()
    {
        await using var app = await TestApp.StartAsync();

        Logger(app, "Kmic.Test").LogInformation("hello");

        var logEvent = app.Logs.Events.Single(e => e.HasTemplate("hello"));
        logEvent.Text("Service").ShouldBe("kmic");
        logEvent.Text("Roles").ShouldBe("Api");
        logEvent.Text("Instance").ShouldNotBeNullOrEmpty();
        logEvent.Text("Version").ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task SensitiveProperty_MaskedInEvents()
    {
        await using var app = await TestApp.StartAsync();

        Logger(app, "Kmic.Test").LogInformation("login {Password}", "p@ss");

        app.Logs.Events.Single(e => e.HasTemplate("login {Password}")).Text("Password").ShouldBe("***");
    }

    [Theory]
    [InlineData("Microsoft.Hosting.Lifetime", LogLevel.Information, false)]
    [InlineData("Microsoft.Hosting.Lifetime", LogLevel.Warning, true)]
    [InlineData("System.Net.Http.HttpClient", LogLevel.Information, false)]
    [InlineData("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information, false)]
    [InlineData("Kmic.Anything", LogLevel.Information, true)]
    [InlineData("Kmic.Anything", LogLevel.Debug, false)]
    public async Task DefaultLevels(string category, LogLevel level, bool logged)
    {
        await using var app = await TestApp.StartAsync();

        Logger(app, category).Log(level, "probe-marker");

        app.Logs.Events.Any(e => e.HasTemplate("probe-marker")).ShouldBe(logged);
    }

    [Fact]
    public async Task LevelsCanBeOverriddenByConfiguration()
    {
        await using var app = await TestApp.StartAsync(configuration: new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Serilog:MinimumLevel:Override:Kmic.Noisy"] = "Error",
        });

        Logger(app, "Kmic.Noisy").LogWarning("probe-quiet");
        Logger(app, "Kmic.Noisy").LogError("probe-loud");

        app.Logs.Events.ShouldContain(e => e.HasTemplate("probe-loud"));
        app.Logs.Events.ShouldNotContain(e => e.HasTemplate("probe-quiet"));
    }

    [Fact]
    public async Task ScopeProperties_AppearOnTheEvent()
    {
        await using var app = await TestApp.StartAsync();
        var logger = Logger(app, "Kmic.Test");

        using (logger.BeginScope(new Dictionary<string, object?>(StringComparer.Ordinal) { ["Origin"] = "Http", ["Module"] = "Sample" }))
        {
            logger.LogInformation("inside");
        }

        var logEvent = app.Logs.Events.Single(e => e.HasTemplate("inside"));
        logEvent.Text("Origin").ShouldBe("Http");
        logEvent.Text("Module").ShouldBe("Sample");
    }

    private static ILogger Logger(TestApp app, string category) => app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(category);
}
