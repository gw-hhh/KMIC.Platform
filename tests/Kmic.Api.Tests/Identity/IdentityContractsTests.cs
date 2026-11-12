using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Identity.Contracts;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// Identity 对外的 Contracts：其他模块解析人员、角色、组织单元的名称，Notifications 取联系方式
/// </summary>
public sealed class IdentityContractsTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private AsyncServiceScope AsUser() => Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateScope(TestIdentity.User("调用方"));

    private AsyncServiceScope AsSystem() => Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope();

    [Fact]
    public async Task UserReferences_ShowNameAndEmployeeNo_AndADisabledUserIsStillResolved()
    {
        var active = await Seed.UserAsync("ref.active", null, displayName: "引用张三", employeeNo: "E-9001");
        var disabled = await Seed.UserAsync("ref.disabled", null, displayName: "引用已离职", active: false);

        await using var scope = AsUser();
        var found = await scope.ServiceProvider.GetRequiredService<IUserReferences>().ResolveAsync([active, disabled, Guid.CreateVersion7(), active], Ct);

        found.Count.ShouldBe(2);
        found[active].ShouldBe(new ReferenceInfo(active, "引用张三", "E-9001", ReferenceStatus.Active));
        found[disabled].DisplayName.ShouldBe("引用已离职");
        found[disabled].Code.ShouldBeNull();
        found[disabled].Status.ShouldBe(ReferenceStatus.Disabled);
    }

    [Fact]
    public async Task UserReferences_ResolveTheSystemAccountToo_SoAuditTrailsShowWho()
    {
        await using var scope = AsUser();

        var found = await scope.ServiceProvider.GetRequiredService<IUserReferences>().ResolveAsync([new Guid("00000000-0000-0000-0000-000000000001")], Ct);

        found.ShouldHaveSingleItem().Value.Status.ShouldBe(ReferenceStatus.Active);
    }

    [Fact]
    public async Task RoleReferences_ShowTheName_AndAStoppedRoleIsStillResolved()
    {
        var active = await Seed.RoleAsync("引用-销售", []);
        var stopped = await Seed.RoleAsync("引用-已停用", [], active: false);

        await using var scope = AsUser();
        var found = await scope.ServiceProvider.GetRequiredService<IRoleReferences>().ResolveAsync([active, stopped], Ct);

        found[active].ShouldBe(new ReferenceInfo(active, "引用-销售", null, ReferenceStatus.Active));
        found[stopped].Status.ShouldBe(ReferenceStatus.Disabled);
    }

    [Fact]
    public async Task OrgUnitReferences_ShowNameAndCode()
    {
        var root = await Seed.OrgTreeAsync();

        await using var scope = AsUser();
        var found = await scope.ServiceProvider.GetRequiredService<IOrgUnitReferences>().ResolveAsync([root, Guid.CreateVersion7()], Ct);

        var reference = found.ShouldHaveSingleItem().Value;
        reference.Id.ShouldBe(root);
        reference.DisplayName.ShouldBe("总部");
        reference.Status.ShouldBe(ReferenceStatus.Active);
    }

    [Fact]
    public async Task Contacts_AreOnlyForTheSystemIdentity()
    {
        var user = await Seed.UserAsync("ref.contact", null, displayName: "联系人", mobile: "+8613800138099", email: "contact@example.com");
        var gone = await Seed.UserAsync("ref.contact.gone", null, displayName: "已离职的联系人", active: false);

        await using (var system = AsSystem())
        {
            var found = await system.ServiceProvider.GetRequiredService<IUserContacts>().GetAsync([user, gone, Guid.CreateVersion7()], Ct);

            found.Count.ShouldBe(2);
            found[user].ShouldBe(new UserContact(user, "联系人", "contact@example.com", "+8613800138099", IsActive: true));
            found[gone].IsActive.ShouldBeFalse();
        }

        await using var caller = AsUser();
        var denied = await Should.ThrowAsync<InvalidOperationException>(async () => await caller.ServiceProvider.GetRequiredService<IUserContacts>().GetAsync([user], Ct));
        denied.Message.ShouldContain("Common.MissingPermission");
    }

    [Fact]
    public async Task ManyIds_AreResolvedInBatches()
    {
        var known = await Seed.UserAsync("ref.batch", null, displayName: "分批的人");
        var ids = Enumerable.Range(0, 2500).Select(_ => Guid.CreateVersion7()).Append(known).ToList();

        await using var scope = AsUser();
        var found = await scope.ServiceProvider.GetRequiredService<IUserReferences>().ResolveAsync(ids, Ct);

        found.ShouldContainKey(known);
    }
}
