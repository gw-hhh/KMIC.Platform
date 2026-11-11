using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Endpoints;

public sealed class DeprecationTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task DeprecatedEndpoint_SendsDeprecationAndSunsetHeaders()
    {
        await using var api = await StartAsync();

        var response = await api.Client.GetAsync("/api/v1/samples/old-me", Ct);

        response.IsSuccessStatusCode.ShouldBeTrue();

        // 2026-11-04 00:00 UTC 的 Unix 秒数；Sunset 是 HTTP 日期
        response.Headers.GetValues("Deprecation").ShouldHaveSingleItem().ShouldBe("@1793750400");
        response.Headers.GetValues("Sunset").ShouldHaveSingleItem().ShouldBe("Tue, 04 May 2027 00:00:00 GMT");
    }

    [Fact]
    public async Task HeadersAreAlsoSentWhenTheRequestFails()
    {
        await using var api = await StartAsync();
        api.Script.On<GetThingQuery, ThingDto>((_, _) => CommonErrors.MissingPermission);

        var response = await api.Client.GetAsync("/api/v1/samples/old-failing", Ct);

        await response.ShouldBeProblemAsync(403, "Common.MissingPermission");
        response.Headers.Contains("Deprecation").ShouldBeTrue();
        response.Headers.Contains("Sunset").ShouldBeTrue();
    }

    [Fact]
    public async Task CurrentEndpoint_HasNoSuchHeaders()
    {
        await using var api = await StartAsync();

        var response = await api.Client.GetAsync("/api/v1/samples/me", Ct);

        response.Headers.Contains("Deprecation").ShouldBeFalse();
        response.Headers.Contains("Sunset").ShouldBeFalse();
    }

    [Fact]
    public async Task Metadata_CarriesBothDates()
    {
        await using var api = await StartAsync();

        var endpoint = api.App.Services.GetServices<EndpointDataSource>().SelectMany(s => s.Endpoints).Single(e => e.DisplayName!.Contains("old-me", StringComparison.Ordinal));

        endpoint.Metadata.GetMetadata<DeprecationMetadata>().ShouldBe(new DeprecationMetadata(new DateOnly(2026, 11, 4), new DateOnly(2027, 5, 4)));
    }

    [Fact]
    public void SunsetBeforeDeprecation_IsRejected()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();

        Should.Throw<ArgumentOutOfRangeException>(() => app.MapGet("/x", () => "x").Deprecated(new DateOnly(2027, 1, 1), new DateOnly(2026, 1, 1)));
    }

    private static async Task<ApiHost> StartAsync()
    {
        var api = await ApiHost.StartAsync([new ListedModule("Samples", DeprecatedEndpoints.All)]);
        api.Script.On<MeQuery, string>((_, _) => "alice");
        return api;
    }
}
