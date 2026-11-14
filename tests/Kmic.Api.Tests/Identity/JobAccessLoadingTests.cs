using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 后台作业按提交人加载访问上下文：没有会话，只看用户、授予和角色现在的状态
/// </summary>
public sealed class JobAccessLoadingTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly string[] _permissions = [OperationsPermissions.AuditExport];

    private Task<IAccessContext?> LoadAsync(Guid user, Guid role) =>
        Factory.Services.GetRequiredService<IAccessContextLoader>().LoadForJobAsync(UserId.From(user), RoleId.From(role), Ct);

    [Fact]
    public async Task AnActiveUserWithTheRole_GetsItsPermissions()
    {
        var role = await Seed.RoleAsync("作业角色", _permissions);
        var user = await Seed.UserAsync("job-loader-active", Password);
        await Seed.GrantAsync(user, role);

        var access = await LoadAsync(user, role);

        access.ShouldNotBeNull();
        access.UserId.Value.ShouldBe(user);
        access.RoleId.Value.ShouldBe(role);
        access.Permissions.Contains(OperationsPermissions.AuditExport).ShouldBeTrue();
        access.ImpersonatorUserId.ShouldBeNull();
    }

    [Fact]
    public async Task ADisabledUser_AnUnknownGrant_OrADisabledRole_GetNothing()
    {
        var role = await Seed.RoleAsync("会被停用的角色", _permissions);
        var otherRole = await Seed.RoleAsync("没授予的角色", _permissions);
        var disabled = await Seed.UserAsync("job-loader-disabled", Password, active: false);
        var withoutGrant = await Seed.UserAsync("job-loader-no-grant", Password);
        var roleDisabled = await Seed.UserAsync("job-loader-role-off", Password);
        await Seed.GrantAsync(disabled, role);
        await Seed.GrantAsync(roleDisabled, role);
        await Seed.SetRoleStatusAsync(role, active: false);

        (await LoadAsync(disabled, role)).ShouldBeNull("用户已停用");
        (await LoadAsync(withoutGrant, otherRole)).ShouldBeNull("这个用户没有被授予这个角色");
        (await LoadAsync(roleDisabled, role)).ShouldBeNull("角色已停用");
    }
}
