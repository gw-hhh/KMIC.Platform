using System.Net;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 同时禁用多个管理员时不能一个都不剩：这条规则数的是整个库里的持有人，所以独占一个数据库
/// </summary>
public sealed class LastProtectedHolderRaceTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly string[] _manage = [IdentityPermissions.UserView, IdentityPermissions.UserManage];

    private async Task<HttpResponseMessage> DisableAsync(string token, Guid id)
    {
        var detail = await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{id}", token));
        return await SendAsync(HttpMethod.Post, $"/users/{id}/disable", token, new { expectedVersion = detail.GetProperty("version").GetInt64(), reason = (string?)null });
    }

    [Fact]
    public async Task ConcurrentDisables_NeverLeaveNoEnabledHolder()
    {
        var token = await LoginAsOperatorAsync("race-operator", _manage);
        var role = await Seed.RoleAsync("并发时的系统管理员", [], isProtected: true);
        var holders = new List<Guid>();
        for (var i = 0; i < 6; i++)
        {
            var user = await Seed.UserAsync($"race.holder{i}", Password, displayName: $"管理员 {i}");
            await Seed.GrantAsync(user, role);
            holders.Add(user);
        }

        var results = await Task.WhenAll(holders.Select(id => DisableAsync(token, id)));

        results.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(5);
        var refused = results.Single(r => r.StatusCode != HttpStatusCode.OK);
        refused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(refused)).GetProperty("code").GetString().ShouldBe("Identity.LastProtectedHolder");
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."User" WHERE "Status" = 'Active' AND "Id" IN (SELECT "UserId" FROM "Identity"."UserRoleGrant" g JOIN "Identity"."Role" r ON r."Id" = g."RoleId" WHERE r."IsProtected")""")).ShouldBe(1);
    }
}
