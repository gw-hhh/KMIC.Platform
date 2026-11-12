using System.Net;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Http;

/// <summary>
/// 带 ETag 的查询：第一次返回内容和 ETag，带着 ETag 再问没变就是 304
/// </summary>
public sealed class ConditionalGetTests : IAsyncLifetime
{
    private ApiHost _api = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => _api = await ApiHost.StartAsync([ConditionalEndpoints.Module()]);

    public async ValueTask DisposeAsync() => await _api.DisposeAsync();

    private async Task<HttpResponseMessage> GetAsync(ThingId id, string? ifNoneMatch = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/conditional/things/{id.Value}");
        if (ifNoneMatch is not null)
        {
            request.Headers.TryAddWithoutValidation("If-None-Match", ifNoneMatch);
        }

        return await _api.Client.SendAsync(request, Ct);
    }

    [Fact]
    public async Task FirstRequest_ReturnsTheBodyAndAnETag_ThatMustBeRevalidated()
    {
        var id = ThingId.New();
        _api.Script.On<GetThingQuery, ThingDto>((q, _) => new ThingDto(q.Id, "v1"));

        var response = await GetAsync(id);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag!.Tag.ShouldBe("\"v1\"");
        response.Headers.CacheControl!.Private.ShouldBeTrue();
        response.Headers.CacheControl.NoCache.ShouldBeTrue();
        (await response.JsonAsync()).GetProperty("name").GetString().ShouldBe("v1");
    }

    [Theory]
    [InlineData("\"v1\"")]
    [InlineData("W/\"v1\"")]
    [InlineData("\"other\", \"v1\"")]
    [InlineData("*")]
    public async Task ASameETag_GivesNotModified_WithoutABody(string presented)
    {
        _api.Script.On<GetThingQuery, ThingDto>((q, _) => new ThingDto(q.Id, "v1"));

        var response = await GetAsync(ThingId.New(), presented);

        response.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        response.Headers.ETag!.Tag.ShouldBe("\"v1\"");
        (await response.Content.ReadAsStringAsync(Ct)).ShouldBeEmpty();
    }

    [Fact]
    public async Task ADifferentETag_GetsTheNewContent()
    {
        _api.Script.On<GetThingQuery, ThingDto>((q, _) => new ThingDto(q.Id, "v2"));

        var response = await GetAsync(ThingId.New(), "\"v1\"");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag!.Tag.ShouldBe("\"v2\"");
    }

    [Fact]
    public async Task AFailure_IsStillAProblem_WithoutAnETag()
    {
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => CommonErrors.MissingPermission);

        var response = await GetAsync(ThingId.New(), "*");

        await response.ShouldBeProblemAsync(403, "Common.MissingPermission");
        response.Headers.ETag.ShouldBeNull();
    }
}
