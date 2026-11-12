using System.Net;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// “系统里至少保留一个启用的系统管理员”：这条规则数的是整个库里的持有人，所以这几个测试独占一个数据库
/// </summary>
public sealed class LastProtectedHolderTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly string[] _manage = [IdentityPermissions.UserView, IdentityPermissions.UserManage];

    private async Task<long> VersionAsync(string token, Guid id) =>
        (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{id}", token))).GetProperty("version").GetInt64();

    private async Task<HttpResponseMessage> DisableAsync(string token, Guid id) =>
        await SendAsync(HttpMethod.Post, $"/users/{id}/disable", token, new { expectedVersion = await VersionAsync(token, id), reason = (string?)null });

    private async Task<Guid> HolderAsync(string account, Guid protectedRole)
    {
        var user = await Seed.UserAsync(account, Password, displayName: "管理员 " + account);
        await Seed.GrantAsync(user, protectedRole);
        return user;
    }

    [Fact]
    public async Task TheLastEnabledHolder_CannotBeDisabled_UntilAnotherOneIsEnabled()
    {
        var token = await LoginAsOperatorAsync("last-operator", _manage);
        var role = await Seed.RoleAsync("系统管理员", [], isProtected: true, builtIn: true);
        var first = await HolderAsync("last.first", role);
        var second = await HolderAsync("last.second", role);

        var disableFirst = await DisableAsync(token, first);
        var disableSecond = await DisableAsync(token, second);

        disableFirst.StatusCode.ShouldBe(HttpStatusCode.OK);
        disableSecond.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(disableSecond)).GetProperty("code").GetString().ShouldBe("Identity.LastProtectedHolder");
        var detail = await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{second}", token));
        ActionsOf(detail).Contains("Disable", StringComparer.Ordinal).ShouldBeFalse();

        (await SendAsync(HttpMethod.Post, $"/users/{first}/enable", token, new { expectedVersion = await VersionAsync(token, first) })).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await DisableAsync(token, second)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
