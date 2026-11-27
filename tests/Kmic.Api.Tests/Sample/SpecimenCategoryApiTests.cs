using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Sample;

public sealed class SpecimenCategoryApiTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/sample/specimen-categories";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly TestIdentity _manager = TestIdentity.User("分类管理员").InTree(_org).InOrgUnit(_org["总部"])
        .WithRole(r => r.Permissions(SamplePermissions.View, SamplePermissions.CategoryManage).Scope(ScopeLevel.All));

    [Fact]
    public async Task Create_Returns201_AndTheListShowsItEnabled()
    {
        using var client = ManagerClient();
        var name = UniqueName("耗材");

        var response = await CreateAsync(client, name);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var id = (await JsonAsync(response)).GetProperty("id").GetGuid();
        response.Headers.Location!.OriginalString.ShouldBe($"{Root}/{id}");
        var item = await FindInListAsync(client, name);
        item.GetProperty("id").GetGuid().ShouldBe(id);
        item.GetProperty("isEnabled").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task Create_SameNameIgnoringCase_Returns409()
    {
        using var client = ManagerClient();
        var name = UniqueName("Box");
        (await CreateAsync(client, name)).EnsureSuccessStatusCode();

        var response = await CreateAsync(client, name.ToUpperInvariant());

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Sample.CategoryNameExists");
    }

    [Fact]
    public async Task Update_ChangesNameAndEnabled_AndAStaleVersionIsRefused()
    {
        using var client = ManagerClient();
        var (id, version) = await CreatedAsync(client, UniqueName("试剂"));
        var renamed = UniqueName("化学试剂");

        var updated = await client.PutAsJsonAsync($"{Root}/{id}", new { name = renamed, isEnabled = false, expectedVersion = version }, Ct);
        var stale = await client.PutAsJsonAsync($"{Root}/{id}", new { name = renamed, isEnabled = true, expectedVersion = version }, Ct);

        updated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(updated)).GetProperty("version").GetInt64().ShouldBe(version + 1);
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(stale)).GetProperty("code").GetString().ShouldBe("Common.VersionConflict");
        (await FindInListAsync(client, renamed)).GetProperty("isEnabled").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task Delete_Returns204_ThenTheCategoryIsGone()
    {
        using var client = ManagerClient();
        var (id, version) = await CreatedAsync(client, UniqueName("包装"));

        var deleted = await DeleteAsync(client, id, version);
        var again = await DeleteAsync(client, id, version);

        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        again.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(again)).GetProperty("code").GetString().ShouldBe("Sample.CategoryNotFound");
    }

    [Fact]
    public async Task WithoutTheManagePermission_CanListButNotChange()
    {
        using var manager = ManagerClient();
        var (id, version) = await CreatedAsync(manager, UniqueName("样品"));
        using var alice = ClientFor(Alice);

        var listed = await alice.GetAsync(Root, Ct);
        var created = await CreateAsync(alice, UniqueName("越权"));
        var deleted = await DeleteAsync(alice, id, version);

        listed.StatusCode.ShouldBe(HttpStatusCode.OK);
        created.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        deleted.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static string UniqueName(string prefix) => $"{prefix}-{Guid.CreateVersion7():N}"[..30];

    private static async Task<HttpResponseMessage> CreateAsync(HttpClient client, string name)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Root) { Content = JsonContent.Create(new { name }) };
        request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());
        return await client.SendAsync(request, Ct);
    }

    private static async Task<(Guid Id, long Version)> CreatedAsync(HttpClient client, string name)
    {
        var response = await CreateAsync(client, name);
        response.EnsureSuccessStatusCode();
        var body = await JsonAsync(response);
        return (body.GetProperty("id").GetGuid(), body.GetProperty("version").GetInt64());
    }

    private static async Task<HttpResponseMessage> DeleteAsync(HttpClient client, Guid id, long version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Root}/{id}") { Content = JsonContent.Create(new { expectedVersion = version }) };
        return await client.SendAsync(request, Ct);
    }

    private static async Task<JsonElement> FindInListAsync(HttpClient client, string name)
    {
        var body = await JsonAsync(await client.GetAsync($"{Root}?q={Uri.EscapeDataString(name)}", Ct));
        return body.GetProperty("items").EnumerateArray().Single(i => string.Equals(i.GetProperty("name").GetString(), name, StringComparison.Ordinal));
    }

    private HttpClient ManagerClient()
    {
        Factory.Loader.Register(_manager);
        return ClientFor(_manager);
    }
}
