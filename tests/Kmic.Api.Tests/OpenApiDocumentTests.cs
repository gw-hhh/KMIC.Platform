using System.Text.Json;
using Kmic.BuildingBlocks.Presentation.OpenApi;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Api.Tests;

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
            .ShouldBe(["Configuration.Dictionary.Manage", "Configuration.Preference.Manage", "Configuration.Setting.Manage", "Configuration.Setting.View", "Files.File.Manage", "Files.File.Purge", "Files.File.View", "Identity.Authorization.Explain", "Identity.OrgUnit.Manage", "Identity.Role.Manage", "Identity.Role.View", "Identity.SecurityEvent.View", "Identity.User.ForceLogout", "Identity.User.Grant", "Identity.User.Impersonate", "Identity.User.ImpersonateWrite", "Identity.User.Manage", "Identity.User.ResetPassword", "Identity.User.Unlock", "Identity.User.View", "Notifications.Captured.View", "Notifications.Delivery.Manage", "Notifications.Delivery.View", "Notifications.Template.Manage", "Notifications.Template.View", "Operations.Audit.Export", "Operations.Audit.View", "Operations.Jobs.Manage", "Operations.Jobs.View", "Operations.Messages.Manage", "Operations.Messages.View", "Operations.MessagingCapacity.Manage", "Sample.Category.Manage", "Sample.Setting.Manage", "Sample.Specimen.Close", "Sample.Specimen.Create", "Sample.Specimen.Delete", "Sample.Specimen.Edit", "Sample.Specimen.Submit", "Sample.Specimen.View"]);
        document.RootElement.GetProperty("x-kmic-enums").GetProperty("SpecimenStatus").GetArrayLength().ShouldBe(3);
    }

    [Fact]
    public async Task ProductionDocument_IsNotExposedWhenNotInProduction_ButIsHere()
    {
        // 测试环境开放 OpenAPI 文档与界面；生产环境禁用由宿主环境判断逻辑保证
        using var client = ClientFor();

        (await client.GetAsync("/openapi/v1.json", Ct)).IsSuccessStatusCode.ShouldBeTrue();
    }

    // 页面引用文档的方式由 Scalar 内部决定，此处仅验证页面可正常访问
    [Fact]
    public async Task ReferencePage_IsServedWhenNotInProduction()
    {
        using var client = ClientFor();

        var response = await client.GetAsync("/scalar", Ct);

        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
    }

    // 前端类型生成与破坏性变更检查（oasdiff）均依赖此文件；由于生成工具会启动完整宿主并连接数据库，由本测试负责导出
    [Fact]
    public async Task Document_IsExportedToTheArtifactsFolder()
    {
        using var client = ClientFor();

        var json = await client.GetStringAsync("/openapi/v1.json", Ct);

        var output = Path.Combine(RepositoryRoot(), "artifacts", "openapi", "openapi.v1.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        await File.WriteAllTextAsync(output, json, Ct);
        new FileInfo(output).Length.ShouldBeGreaterThan(0);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Kmic.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("找不到 Kmic.slnx。");
    }
}
