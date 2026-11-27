using Kmic.Notifications.Application;
using Kmic.Notifications.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

public sealed class CaptureConfigurationTests
{
    private static IOptions<NotificationsCaptureOptions> Resolve(string environment, bool? enabled)
    {
        var settings = new Dictionary<string, string?>(StringComparer.Ordinal);
        if (enabled is { } value)
        {
            settings[$"{NotificationsCaptureOptions.Section}:Enabled"] = value.ToString();
        }

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        services.AddSingleton<IHostEnvironment>(new FakeEnvironment(environment));
        new NotificationsModule().AddServices(services);
        return services.BuildServiceProvider().GetRequiredService<IOptions<NotificationsCaptureOptions>>();
    }

    [Fact]
    public void InProduction_LeftOnByDefault_IsRefused()
    {
        var options = Resolve(Environments.Production, enabled: null);

        Should.Throw<OptionsValidationException>(() => _ = options.Value)
            .Message.ShouldContain("Notifications:Capture:Enabled");
    }

    [Fact]
    public void InProduction_SwitchedOff_IsAccepted() =>
        Resolve(Environments.Production, enabled: false).Value.Enabled.ShouldBeFalse();

    [Fact]
    public void OutsideProduction_IsOnByDefault() =>
        Resolve(Environments.Staging, enabled: null).Value.Enabled.ShouldBeTrue();

    private sealed class FakeEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;

        public string ApplicationName { get; set; } = "Kmic.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
