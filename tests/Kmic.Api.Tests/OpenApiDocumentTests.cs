using System.Text.Json;
using Kmic.BuildingBlocks.Presentation.OpenApi;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Api.Tests;

/// <summary>
/// 真实宿主生成的 OpenAPI 文档：XML 注释要真的出现在文档里，前端才看得懂
/// </summary>
public sealed class OpenApiDocumentTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    [Fact]
    public async Task Document_IsComplete()
    {
        using var client = ClientFor();

        var response = await client.GetAsync("/openapi/v1.json", Ct);

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        OpenApiCompleteness.Check(document.RootElement).ShouldBeEmpty();
    }

    [Fact]
    public async Task Document_ListsTheSampleOperationsWithTheirPermissions()
    {
        using var client = ClientFor();

        var response = await client.GetAsync("/openapi/v1.json", Ct);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        var paths = document.RootElement.GetProperty("paths");
        paths.GetProperty("/api/v1/sample/specimens").GetProperty("post").GetProperty("x-permission").GetString().ShouldBe("Sample.Specimen.Create");
        paths.GetProperty("/api/v1/sample/specimens").GetProperty("get").GetProperty("x-sortable-fields").EnumerateArray().Select(f => f.GetString()).ShouldBe(["number", "title", "createdAt"]);
        paths.GetProperty("/api/v1/sample/specimens/{id}/submit").GetProperty("post").GetProperty("x-permission").GetString().ShouldBe("Sample.Specimen.Submit");
        document.RootElement.GetProperty("x-kmic-permissions").EnumerateArray().Select(p => p.GetProperty("code").GetString())
            .ShouldBe(["Sample.Specimen.Close", "Sample.Specimen.Create", "Sample.Specimen.Delete", "Sample.Specimen.Submit", "Sample.Specimen.View"]);
        document.RootElement.GetProperty("x-kmic-enums").GetProperty("SpecimenStatus").GetArrayLength().ShouldBe(3);
    }

    [Fact]
    public async Task ProductionDocument_IsNotExposedWhenNotInProduction_ButIsHere()
    {
        // 测试环境开放文档与界面；生产环境不开放由宿主的环境判断保证
        using var client = ClientFor();

        (await client.GetAsync("/openapi/v1.json", Ct)).IsSuccessStatusCode.ShouldBeTrue();
    }
}
