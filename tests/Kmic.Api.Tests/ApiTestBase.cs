using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Sample.Application;

namespace Kmic.Api.Tests;

// 组织架构：总部下设华东、华南。甲（华东，Sample 全权限，仅本人）；乙（华东，提交+查看，全部）；丙（华南，查看+创建，仅本人）
public abstract class ApiTestBase(
    ClassDatabase<ApiTemplate> database,
    ApiTemplate template,
    IReadOnlyDictionary<string, string>? extraSettings = null,
    Action<IServiceCollection>? configureServices = null) : IAsyncLifetime
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部", hq => hq.Unit("华东").Unit("华南")));

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    protected TestIdentity Alice { get; } = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["华东"]).WithRole(r => r
        .Permissions(SamplePermissions.View, SamplePermissions.Create, SamplePermissions.Edit, SamplePermissions.Submit, SamplePermissions.Close, SamplePermissions.Delete)
        .Scope(ScopeLevel.Self));

    protected TestIdentity Bob { get; } = TestIdentity.User("乙").InTree(_org).InOrgUnit(_org["华东"]).WithRole(r => r
        .Permissions(SamplePermissions.View, SamplePermissions.Submit)
        .Scope(ScopeLevel.All));

    protected TestIdentity Carol { get; } = TestIdentity.User("丙").InTree(_org).InOrgUnit(_org["华南"]).WithRole(r => r
        .Permissions(SamplePermissions.View, SamplePermissions.Create)
        .Scope(ScopeLevel.Self));

    protected ApiFactory Factory { get; private set; } = null!;

    protected string ConnectionString => template.RuntimeConnectionString(database.Name);

    public ValueTask InitializeAsync()
    {
        // 宿主随测试类数据库一并创建与销毁；各测试实例的三个身份 ID 互不相同，注册到同一加载器互不干扰
        Factory = database.Shared(() => new ApiFactory(ConnectionString, extraSettings: extraSettings, configureServices: configureServices));
        foreach (var identity in new[] { Alice, Bob, Carol })
        {
            Factory.Loader.Register(identity);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>创建指定身份的客户端，未传入时匿名</summary>
    protected HttpClient ClientFor(TestIdentity? identity = null)
    {
        var client = Factory.CreateClient();
        if (identity is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Factory.Tokens.Create(identity));
        }

        return client;
    }

    protected static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync(Ct);
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    protected static async Task<(Guid Id, long Version)> CreateAsync(HttpClient client, string title = "第一个示例", params (string Name, int Quantity)[] items)
    {
        var body = new
        {
            title,
            items = (items.Length > 0 ? items : [("样品", 2)]).Select(i => new { name = i.Name, quantity = i.Quantity }),
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/sample/specimens") { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());
        var response = await client.SendAsync(request, Ct);
        response.EnsureSuccessStatusCode();
        var created = await JsonAsync(response);
        return (created.GetProperty("id").GetGuid(), created.GetProperty("version").GetInt64());
    }

    /// <summary>调用带 [Idempotent] 的动作接口，每次自动生成新幂等键</summary>
    protected static async Task<HttpResponseMessage> PostActionAsync(HttpClient client, string url, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());
        return await client.SendAsync(request, Ct);
    }
}
