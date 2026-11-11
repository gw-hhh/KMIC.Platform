using System.ComponentModel.DataAnnotations;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Hosting;

public sealed class AddKmicOptionsTests
{
    [Fact]
    public void ValidSection_BoundAndAvailable()
    {
        using var provider = Build(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Modules:Sample:Import:BatchSize"] = "500",
            ["Modules:Sample:Import:PartnerEndpoint"] = "https://partner.example.com",
        });

        provider.GetRequiredService<IStartupValidator>().Validate();

        var options = provider.GetRequiredService<IOptions<ImportOptions>>().Value;
        options.BatchSize.ShouldBe(500);
        options.PartnerEndpoint.ShouldBe("https://partner.example.com");
    }

    [Fact]
    public void MissingSection_DefaultsApplyButRequiredMembersFailAtStartup()
    {
        using var provider = Build(new Dictionary<string, string?>(StringComparer.Ordinal));

        var failure = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

        failure.Failures.ShouldContain(f => f.Contains("PartnerEndpoint", StringComparison.Ordinal));
    }

    [Fact]
    public void EveryProblemListedTogether()
    {
        using var provider = Build(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Modules:Sample:Import:BatchSize"] = "0",
            ["Modules:Sample:Import:PartnerEndpoint"] = "不是地址",
        });

        var failure = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

        failure.Failures.Count().ShouldBe(2);
        failure.Failures.ShouldContain(f => f.Contains("BatchSize", StringComparison.Ordinal));
        failure.Failures.ShouldContain(f => f.Contains("PartnerEndpoint", StringComparison.Ordinal));
    }

    private static ServiceProvider Build(Dictionary<string, string?> values)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        services.AddKmicOptions<ImportOptions>("Modules:Sample:Import");
        return services.BuildServiceProvider();
    }

    private sealed class ImportOptions
    {
        [Range(1, 10_000)]
        public int BatchSize { get; init; } = 200;

        [Required]
        [Url]
        public string PartnerEndpoint { get; init; } = "";
    }
}
