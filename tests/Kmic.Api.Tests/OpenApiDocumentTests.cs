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
            .ShouldBe(["Configuration.Dictionary.Manage", "Configuration.Preference.Manage", "Configuration.Setting.Manage", "Configuration.Setting.View", "Files.File.Manage", "Files.File.Purge", "Files.File.View", "Identity.OrgUnit.Manage", "Identity.Role.Manage", "Identity.Role.View", "Identity.SecurityEvent.View", "Identity.User.ForceLogout", "Identity.User.Grant", "Identity.User.Impersonate", "Identity.User.ImpersonateWrite", "Identity.User.Manage", "Identity.User.ResetPassword", "Identity.User.Unlock", "Identity.User.View", "Notifications.Captured.View", "Notifications.Delivery.Manage", "Notifications.Delivery.View", "Notifications.Template.Manage", "Notifications.Template.View", "Operations.Audit.Export", "Operations.Audit.View", "Operations.Jobs.Manage", "Operations.Jobs.View", "Operations.Messages.Manage", "Operations.Messages.View", "Sample.Setting.Manage", "Sample.Specimen.Close", "Sample.Specimen.Create", "Sample.Specimen.Delete", "Sample.Specimen.Edit", "Sample.Specimen.Submit", "Sample.Specimen.View"]);
        document.RootElement.GetProperty("x-kmic-enums").GetProperty("SpecimenStatus").GetArrayLength().ShouldBe(3);
    }

    [Fact]
    public async Task ProductionDocument_IsNotExposedWhenNotInProduction_ButIsHere()
    {
        // 测试环境开放文档与界面；生产环境不开放由宿主的环境判断保证
        using var client = ClientFor();

        (await client.GetAsync("/openapi/v1.json", Ct)).IsSuccessStatusCode.ShouldBeTrue();
    }

    // 前端生成类型、破坏性变更检查（oasdiff）都读这个文件。文档由真实宿主生成：构建时生成的工具会启动整个宿主，
    // 启动任务要连数据库，所以改由这里导出
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
