using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Hosting;

/// <summary>
/// 框架参数从部署配置绑定后，启动时就检查：写错的容量保护配置不能带着上线
/// </summary>
public sealed class FrameworkOptionsValidationTests
{
    [Fact]
    public void NoAdmissionConfiguration_StartsWithTheDefaults()
    {
        using var provider = Build(new Dictionary<string, string?>(StringComparer.Ordinal));

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact]
    public void ContradictoryAdmissionConfiguration_FailsAtStartupAndNamesTheSetting()
    {
        using var provider = Build(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Kmic:Messaging:Admission:SoftRatio"] = "0.97",
            ["Kmic:Messaging:Admission:HardRatio"] = "0.9",
        });

        var failure = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

        failure.Failures.ShouldContain(f => f.Contains("Kmic:Messaging:Admission:SoftRatio", StringComparison.Ordinal));
    }

    private static ServiceProvider Build(Dictionary<string, string?> values)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        services.AddSingleton<IHostEnvironment>(new DevelopmentEnvironment());
        services.AddKmicFrameworkOptions();
        return services.BuildServiceProvider();
    }

    private sealed class DevelopmentEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "Kmic.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
