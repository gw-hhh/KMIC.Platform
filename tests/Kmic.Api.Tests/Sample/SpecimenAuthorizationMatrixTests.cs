using System.Net;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Sample;

public sealed class SpecimenAuthorizationMatrixTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/sample/specimens";
    private const string Own = "本人";
    private const string SameUnit = "同组织他人";
    private const string BelowUnit = "下级组织他人";
    private const string OtherRegion = "其他区域他人";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部", hq => hq.Unit("华东", east => east.Unit("上海")).Unit("华南")));

    public static TheoryData<ScopeLevel, string[]> Levels => new()
    {
        { ScopeLevel.Self, [Own] },
        { ScopeLevel.OrgUnit, [Own, SameUnit] },
        { ScopeLevel.OrgUnitAndBelow, [Own, SameUnit, BelowUnit] },
        { ScopeLevel.All, [Own, SameUnit, BelowUnit, OtherRegion] },
    };

    [Theory]
    [MemberData(nameof(Levels))]
    public async Task EveryExit_ShowsExactlyTheDataTheLevelAllows(ScopeLevel level, string[] expected)
    {
        // 查看者位于华东；四份测试数据分别由本人、华东同级同事、上海下级及华南人员创建
        var viewer = Member($"查看者-{level}", "华东", level, SamplePermissions.View, SamplePermissions.Create);
        var owners = new Dictionary<string, TestIdentity>(StringComparer.Ordinal)
        {
            [Own] = viewer,
            [SameUnit] = Member("同组织", "华东", ScopeLevel.Self, SamplePermissions.Create),
            [BelowUnit] = Member("下级组织", "上海", ScopeLevel.Self, SamplePermissions.Create),
            [OtherRegion] = Member("其他区域", "华南", ScopeLevel.Self, SamplePermissions.Create),
        };
        foreach (var identity in owners.Values)
        {
            Factory.Loader.Register(identity);
        }

        var tag = $"M{Guid.CreateVersion7():N}";
        var created = new Dictionary<string, Guid>(StringComparer.Ordinal);
        foreach (var (owner, identity) in owners)
        {
            using var creator = ClientFor(identity);
            created[owner] = (await CreateAsync(creator, $"{tag}-{owner}")).Id;
        }

        using var client = ClientFor(viewer);

        var list = await JsonAsync(await client.GetAsync($"{Root}?q={tag}&pageSize=100", Ct));
        var listed = list.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()).ToHashSet();
        created.Where(c => listed.Contains(c.Value)).Select(c => c.Key).Order(StringComparer.Ordinal)
            .ShouldBe(expected.Order(StringComparer.Ordinal), "列表出口");

        foreach (var (owner, id) in created)
        {
            var detail = await client.GetAsync($"{Root}/{id}", Ct);
            detail.StatusCode.ShouldBe(expected.Contains(owner) ? HttpStatusCode.OK : HttpStatusCode.NotFound, $"详情出口：{owner}");
        }
    }

    private static TestIdentity Member(string name, string unit, ScopeLevel level, params string[] permissions) =>
        TestIdentity.User(name).InTree(_org).InOrgUnit(_org[unit]).WithRole(r => r.Permissions(permissions).Scope(level));
}
