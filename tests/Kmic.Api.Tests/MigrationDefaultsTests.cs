using Kmic.BuildingBlocks.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Infrastructure;
using Kmic.Sample.Presentation;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.Api.Tests;

public sealed class MigrationDefaultsTests
{
    [Theory]
    [InlineData("Development", true)]
    [InlineData("Testing", false)]
    [InlineData("Staging", false)]
    public void AutoMigrate_OnlyInDevelopment_ByDefault(string environment, bool expected)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.Configuration["Kmic:Database:ConnectionString"] = "Host=localhost;Database=unused";
        builder.Configuration["Kmic:Roles"] = "Api";
        builder.AddKmicHost(host => host.AddModule<SampleModule, SampleEndpoints>());

        using var provider = builder.Services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.AutoMigrate.ShouldBe(expected);
    }

    [Fact]
    public void AutoMigrate_ConfigurationOverridesDefault()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Staging" });
        builder.Configuration["Kmic:Database:ConnectionString"] = "Host=localhost;Database=unused";
        builder.Configuration["Kmic:Roles"] = "Api";
        builder.Configuration["Kmic:Database:AutoMigrate"] = "true";
        builder.AddKmicHost(host => host.AddModule<SampleModule, SampleEndpoints>());

        using var provider = builder.Services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.AutoMigrate.ShouldBeTrue();
    }
}
