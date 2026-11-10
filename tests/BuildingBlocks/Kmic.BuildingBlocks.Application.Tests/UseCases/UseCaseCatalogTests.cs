using Kmic.BuildingBlocks.Application.Tests.Fixtures;
using Kmic.BuildingBlocks.Application.UseCases;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.UseCases;

public sealed class UseCaseCatalogTests
{
    [Fact]
    public void Build_ValidDeclarations_ReadsEveryAspect()
    {
        var catalog = UseCaseCatalog.Build([typeof(SubmitThingCommand), typeof(ChangePasswordCommand), typeof(LookupThingQuery)]);

        var submit = catalog.Get(typeof(SubmitThingCommand));
        submit.Module.ShouldBe("BuildingBlocks");
        submit.Name.ShouldBe("BuildingBlocks.SubmitThingCommand");
        submit.Kind.ShouldBe(MessageKind.Command);
        submit.Requirement.ShouldBe(AccessRequirement.Permission);
        submit.Permission.ShouldBe(SubmitThingCommand.Permission);
        submit.Audited.ShouldBe(true);
        submit.AuditAction.ShouldBe("BuildingBlocks.SubmitThing");

        var changePassword = catalog.Get(typeof(ChangePasswordCommand));
        changePassword.AllowWhenPasswordChangeRequired.ShouldBeTrue();
        changePassword.Audited.ShouldBe(false);

        var lookup = catalog.Get(typeof(LookupThingQuery));
        lookup.Requirement.ShouldBe(AccessRequirement.ContractQuery);
        lookup.ContractAccess.ShouldBe(ContractAccess.Reference);
        lookup.Audited.ShouldBeNull();
    }

    [Theory]
    [InlineData(typeof(CatalogSamples.NoDeclarationCommand), "必须且只能有")]
    [InlineData(typeof(CatalogSamples.TwoDeclarationsQuery), "现在有 2 个")]
    [InlineData(typeof(CatalogSamples.SystemOnlyWithAllowSystemCommand), "[AllowSystem] 只能")]
    [InlineData(typeof(CatalogSamples.PasswordExemptWithPermissionCommand), "[AllowWhenPasswordChangeRequired] 只能")]
    [InlineData(typeof(CatalogSamples.ContractCommand), "[ContractQuery] 只能用于查询")]
    [InlineData(typeof(CatalogSamples.UnauditedCommand), "[Audited]、[NotAudited] 之一")]
    [InlineData(typeof(CatalogSamples.IdempotentNotAuditedCommand), "必须 [Audited]")]
    [InlineData(typeof(CatalogSamples.IdempotentQuery), "只能用于命令")]
    public void Build_WrongDeclaration_RefusesWithReason(Type messageType, string reason)
    {
        var thrown = Should.Throw<UseCaseCatalogException>(() => UseCaseCatalog.Build([messageType]));

        thrown.Problems.ShouldContain(p => p.Contains(reason, StringComparison.Ordinal) && p.Contains(messageType.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Build_SeveralProblems_ReportsAllAtOnce()
    {
        var thrown = Should.Throw<UseCaseCatalogException>(
            () => UseCaseCatalog.Build([typeof(CatalogSamples.NoDeclarationCommand), typeof(CatalogSamples.IdempotentQuery)]));

        thrown.Problems.Count.ShouldBe(2);
    }

    [Fact]
    public void Get_UnknownMessage_Throws()
    {
        var catalog = UseCaseCatalog.Build([typeof(GetThingQuery)]);

        Should.Throw<InvalidOperationException>(() => catalog.Get(typeof(SubmitThingCommand)));
    }
}
