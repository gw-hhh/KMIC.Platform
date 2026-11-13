using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Sample.Application;

namespace Kmic.Api.Tests;

/// <summary>
/// API 测试的公共部分：每个测试类一个数据库、一个宿主，以及三个身份
/// </summary>
/// <remarks>
/// 组织：总部下有华东、华南。甲在华东，有 Sample 的全部权限，数据范围只看自己；
/// 乙同在华东，有提交和查看权限，数据范围是全部；丙在华南，有查看和创建权限，范围只看自己
/// </remarks>
public abstract class ApiTestBase(ClassDatabase<ApiTemplate> database, ApiTemplate template, IReadOnlyDictionary<string, string>? extraSettings = null) : IAsyncLifetime
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部", hq => hq.Unit("华东").Unit("华南")));

    private readonly TokenFactory _tokens = new();

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

    /// <summary>
    /// 宿主用的运行账号连接串，测试直接查库时也用它
    /// </summary>
    protected string ConnectionString => template.RuntimeConnectionString(database.Name);

    public ValueTask InitializeAsync()
    {
        Factory = new ApiFactory(ConnectionString, _tokens, extraSettings: extraSettings);
        foreach (var identity in new[] { Alice, Bob, Carol })
        {
            Factory.Loader.Register(identity);
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        _tokens.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 以某个身份登录的客户端，不传身份就是没有登录
    /// </summary>
    protected HttpClient ClientFor(TestIdentity? identity = null)
    {
        var client = Factory.CreateClient();
        if (identity is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokens.Create(identity));
        }

        return client;
    }

    protected static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync(Ct);
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// 创建一个标本，返回它的 ID 和版本号
    /// </summary>
    protected static async Task<(Guid Id, long Version)> CreateAsync(HttpClient client, string title = "第一个标本", params (string Name, int Quantity)[] items)
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

    /// <summary>
    /// 以 POST 调用一个带 [Idempotent] 的动作接口，每次换一个新的幂等键
    /// </summary>
    protected static async Task<HttpResponseMessage> PostActionAsync(HttpClient client, string url, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());
        return await client.SendAsync(request, Ct);
    }
}
