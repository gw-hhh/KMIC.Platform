using System.Net;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Sample;

/// <summary>
/// 一致性：详情返回的 allowedActions 包含某个动作，当且仅当执行对应的命令不会因权限、资源策略、状态被拒绝
/// </summary>
public sealed class SpecimenAllowedActionsTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/sample/specimens";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部", hq => hq.Unit("华东")));

    // 动作目录里的每个动作，各对应一个接口
    private static readonly (string Code, string Path)[] _actions = [("Submit", "submit"), ("Close", "close")];

    public static TheoryData<string> States => ["Draft", "Submitted", "Closed"];

    [Theory]
    [MemberData(nameof(States))]
    public async Task AllowedActions_AgreeWithWhatTheCommandsDo(string state)
    {
        // 甲是负责人，有全部权限；乙有提交权限但不是负责人；丁只能查看
        var ding = Viewer("丁", SamplePermissions.View);
        Factory.Loader.Register(ding);

        foreach (var identity in new[] { Alice, Bob, ding })
        {
            foreach (var (code, path) in _actions)
            {
                var (id, version) = await CreateInStateAsync(Alice, state);

                await AssertAgreesAsync(identity, id, version, code, path, $"{identity.DisplayName}，状态 {state}，{code}");
            }
        }
    }

    [Fact]
    public async Task OwnerWithoutThePermissions_GetsNoActionsAndTheCommandsAreRefused()
    {
        var wu = Viewer("戊", SamplePermissions.View, SamplePermissions.Create);
        Factory.Loader.Register(wu);

        foreach (var (code, path) in _actions)
        {
            var (id, version) = await CreateInStateAsync(wu, "Draft");

            await AssertAgreesAsync(wu, id, version, code, path, $"负责人没有权限，{code}");
        }
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Submitted")]
    public async Task OwnerWhoCanSubmitButNotClose_GetsOnlyTheActionsHeIsEntitledTo(string state)
    {
        // 动作目录里 Close 绑错权限点时，只有这个组合会露出来：他是负责人，有提交权限，没有关闭权限
        var ji = Viewer("己", SamplePermissions.View, SamplePermissions.Create, SamplePermissions.Submit);
        Factory.Loader.Register(ji);

        foreach (var (code, path) in _actions)
        {
            var (id, version) = await CreateInStateAsync(ji, state);

            await AssertAgreesAsync(ji, id, version, code, path, $"负责人有提交权限，状态 {state}，{code}");
        }
    }

    private static TestIdentity Viewer(string name, params string[] permissions) =>
        TestIdentity.User(name).InTree(_org).InOrgUnit(_org["华东"]).WithRole(r => r.Permissions(permissions).Scope(ScopeLevel.All));

    private static async Task<long> AdvanceAsync(HttpClient client, Guid id, string path, long version)
    {
        var response = await PostActionAsync(client, $"{Root}/{id}/{path}", new { expectedVersion = version });
        response.EnsureSuccessStatusCode();
        return (await JsonAsync(response)).GetProperty("version").GetInt64();
    }

    // 负责人创建标本并推进到指定状态；推进用的是负责人自己的权限
    private async Task<(Guid Id, long Version)> CreateInStateAsync(TestIdentity owner, string state)
    {
        using var client = ClientFor(owner);
        var (id, version) = await CreateAsync(client, "动作一致性");
        if (state is "Submitted" or "Closed")
        {
            version = await AdvanceAsync(client, id, "submit", version);
        }

        if (string.Equals(state, "Closed", StringComparison.Ordinal))
        {
            version = await AdvanceAsync(client, id, "close", version);
        }

        return (id, version);
    }

    private async Task AssertAgreesAsync(TestIdentity identity, Guid id, long version, string code, string path, string context)
    {
        using var client = ClientFor(identity);
        var detail = await JsonAsync(await client.GetAsync($"{Root}/{id}", Ct));
        var allowed = detail.GetProperty("allowedActions").EnumerateArray().Select(a => a.GetString()).Contains(code, StringComparer.Ordinal);

        var response = await PostActionAsync(client, $"{Root}/{id}/{path}", new { expectedVersion = version });

        (response.StatusCode == HttpStatusCode.OK).ShouldBe(allowed, context);
        if (!allowed)
        {
            response.StatusCode.ShouldBeOneOf([HttpStatusCode.Forbidden, HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity], context);
        }
    }
}
