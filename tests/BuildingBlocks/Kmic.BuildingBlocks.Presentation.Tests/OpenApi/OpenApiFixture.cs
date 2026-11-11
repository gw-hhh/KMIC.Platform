using System.Text.Json;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Dictionaries;
using Kmic.BuildingBlocks.Application.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.OpenApi;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;

namespace Kmic.BuildingBlocks.Presentation.Tests.OpenApi;

/// <summary>
/// 生成一次文档，OpenApiDocumentTests 的各个测试读同一份
/// </summary>
public sealed class OpenApiFixture : IAsyncLifetime
{
    private ApiHost _api = null!;

    internal JsonElement Document { get; private set; }

    public async ValueTask InitializeAsync()
    {
        _api = await ApiHost.StartAsync(
            [
                ThingEndpoints.Module("Samples"),
                new ListedModule("Identity", typeof(BrokenEndpoints.AnonymousEndpoint)),
                new ListedModule("Legacy", typeof(DeprecatedEndpoints.OldMeEndpoint)),
            ],
            services: services =>
            {
                services.AddOpenApi("v1", options => options.AddKmicConventions());
                services.AddSingleton(PermissionCatalog.Build([typeof(Permissions)]));
                services.AddSingleton(ErrorCatalog.Build([typeof(TestErrors)]));
                services.AddSingleton(DictionaryTypeCatalog.Build([typeof(Dictionaries)]));
            },
            beforeEndpoints: app => app.MapOpenApi(),

            // 匿名端点只允许在 Identity 模块的用例里，测试里的消息不在那里
            skipValidation: true);

        var response = await _api.Client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Document = JsonDocument.Parse(text).RootElement.Clone();
    }

    public async ValueTask DisposeAsync() => await _api.DisposeAsync();

    [PermissionGroup("Test", "测试")]
    internal static class Permissions
    {
        [Permission("查看测试数据")]
        public const string Read = "Test.Thing.Read";

        [Permission("修改测试数据", Sensitive = true)]
        public const string Write = "Test.Thing.Write";
    }

    internal static class TestErrors
    {
        public static readonly Error Missing = Error.NotFound("Test.Missing", "没有这条数据");
    }

    [DictionaryTypeGroup("Test")]
    internal static class Dictionaries
    {
        [DictionaryType("行业", IsHierarchical = true)]
        public const string Industry = "Test.Industry";
    }
}
