using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Api.Tests.Configuration;

/// <summary>
/// 功能开关：值来自部署配置 Kmic:Features，登录就能查当前开放的开关，端到端
/// </summary>
public sealed class FeatureTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(
        database,
        template,
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Kmic:Features:Sample.NewImport"] = "true",
            ["Kmic:Features:Sample.Preview"] = "false",
            ["Kmic:Features:Orders.Wholesale"] = "true",
        }),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    [Fact]
    public async Task AnyLoggedInUser_SeesTheEnabledFeatures_SortedByName_WithoutTheOnesThatAreOff()
    {
        var token = await LoginAsOperatorAsync("feature-reader", []);

        var response = await SendAsync(HttpMethod.Get, "/api/v1/configuration/features", token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var enabled = (await JsonAsync(response)).GetProperty("enabled").EnumerateArray().Select(e => e.GetString()).ToList();
        enabled.ShouldBe(["Orders.Wholesale", "Sample.NewImport"]);
    }

    [Fact]
    public async Task WithoutLoggingIn_TheListIsRefused()
    {
        using var anonymous = NewClient();

        (await anonymous.GetAsync("/api/v1/configuration/features", Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TheResponseIsAnObject_SoMoreCanBeAddedLater()
    {
        var token = await LoginAsOperatorAsync("feature-shape", []);

        var body = await JsonAsync(await SendAsync(HttpMethod.Get, "/api/v1/configuration/features", token));

        body.ValueKind.ShouldBe(JsonValueKind.Object);
        body.EnumerateObject().Select(p => p.Name).ShouldBe(["enabled"]);
    }
}
