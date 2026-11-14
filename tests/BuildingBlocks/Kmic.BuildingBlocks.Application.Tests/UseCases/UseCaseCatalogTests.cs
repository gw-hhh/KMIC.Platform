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
        changePassword.AllowWhenImpersonationReadOnly.ShouldBeTrue();
        submit.AllowWhenImpersonationReadOnly.ShouldBeFalse();
        changePassword.Audited.ShouldBe(false);

        var lookup = catalog.Get(typeof(LookupThingQuery));
        lookup.Requirement.ShouldBe(AccessRequirement.ContractQuery);
        lookup.ContractAccess.ShouldBe(ContractAccess.Reference);
        lookup.Audited.ShouldBeNull();
    }

    [Theory]
    [InlineData(typeof(CatalogSamples.NoDeclarationCommand), "必须且只能有")]
    [InlineData(typeof(CatalogSamples.BadFeatureNameQuery), "功能开关名")]
    [InlineData(typeof(CatalogSamples.TwoDeclarationsQuery), "现在有 2 个")]
    [InlineData(typeof(CatalogSamples.SystemOnlyWithAllowSystemCommand), "[AllowSystem] 只能")]
    [InlineData(typeof(CatalogSamples.PasswordExemptWithPermissionCommand), "[AllowWhenPasswordChangeRequired] 只能")]
    [InlineData(typeof(CatalogSamples.ImpersonationExemptWithPermissionCommand), "[AllowWhenImpersonationReadOnly] 只能")]
    [InlineData(typeof(CatalogSamples.ImpersonationExemptQuery), "[AllowWhenImpersonationReadOnly] 只能")]
    [InlineData(typeof(CatalogSamples.ContractCommand), "[ContractQuery] 只能用于查询")]
    [InlineData(typeof(CatalogSamples.UnauditedCommand), "[Audited]、[NotAudited] 之一")]
    [InlineData(typeof(CatalogSamples.IdempotentNotAuditedCommand), "必须 [Audited]")]
    [InlineData(typeof(CatalogSamples.IdempotentQuery), "只能用于命令")]
    [InlineData(typeof(CatalogSamples.UnauditedDownloadQuery), "返回 FileDownload 的查询必须 [AuditedRead]")]
    public void Build_WrongDeclaration_RefusesWithReason(Type messageType, string reason)
    {
        var thrown = Should.Throw<DeclarationException>(() => UseCaseCatalog.Build([messageType]));

        thrown.Problems.ShouldContain(p => p.Contains(reason, StringComparison.Ordinal) && p.Contains(messageType.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Build_ReadsTheFeatureGate_AndLeavesItEmptyWhenThereIsNone()
    {
        var catalog = UseCaseCatalog.Build([typeof(GatedThingQuery), typeof(GetThingQuery)]);

        catalog.Get(typeof(GatedThingQuery)).FeatureGate.ShouldBe(GatedThingQuery.Feature);
        catalog.Get(typeof(GetThingQuery)).FeatureGate.ShouldBeNull();
    }

    [Fact]
    public void Build_AuditedFileDownloadQuery_IsAccepted() =>
        UseCaseCatalog.Build([typeof(CatalogSamples.AuditedDownloadQuery)]).Get(typeof(CatalogSamples.AuditedDownloadQuery)).AuditedRead.ShouldBeTrue();

    [Fact]
    public void Build_ContractQueryThatReturnsAFileDownload_NeedsNoReadAudit_BecauseTheBusinessDownloadQueryIsTheAuditedOne() =>
        Should.NotThrow(() => UseCaseCatalog.Build([typeof(CatalogSamples.ContractDownloadQuery)]));

    [Fact]
    public void Build_SeveralProblems_ReportsAllAtOnce()
    {
        var thrown = Should.Throw<DeclarationException>(
            () => UseCaseCatalog.Build([typeof(CatalogSamples.NoDeclarationCommand), typeof(CatalogSamples.IdempotentQuery)]));

        thrown.Problems.Count.ShouldBe(2);
    }

    [Fact]
    public void Get_UnknownMessage_Throws()
    {
        var catalog = UseCaseCatalog.Build([typeof(GetThingQuery)]);

        Should.Throw<InvalidOperationException>(() => catalog.Get(typeof(SubmitThingCommand)));
    }

    [Fact]
    public void Build_HandlerBypassesDataScope_RecordsReasonOnUseCase()
    {
        var catalog = UseCaseCatalog.Build([typeof(CatalogSamples.CheckNumberQuery), typeof(CatalogSamples.CheckNumberQueryHandler), typeof(GetThingQuery)]);

        catalog.Get(typeof(CatalogSamples.CheckNumberQuery)).BypassesDataScope.ShouldBe("编号唯一性要在全部数据中判断");
        catalog.Get(typeof(GetThingQuery)).BypassesDataScope.ShouldBeNull();
    }

    [Fact]
    public void Build_WrongBypassDeclaration_Refuses()
    {
        var thrown = Should.Throw<DeclarationException>(() => UseCaseCatalog.Build(
        [
            typeof(CatalogSamples.BlankBypassReasonQuery),
            typeof(CatalogSamples.BlankBypassReasonQueryHandler),
            typeof(CatalogSamples.NotAHandler),
        ]));

        thrown.Problems.ShouldContain(p => p.Contains(nameof(CatalogSamples.BlankBypassReasonQueryHandler), StringComparison.Ordinal) && p.Contains("必须写明原因", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains(nameof(CatalogSamples.NotAHandler), StringComparison.Ordinal) && p.Contains("只能标在命令或查询处理器上", StringComparison.Ordinal));
    }

    [Fact]
    public void FromAssemblies_DoesNotSkipMalformedTypes()
    {
        // 测试程序集里有故意写错的样例，扫描要把它们都找出来，不能悄悄跳过
        var thrown = Should.Throw<DeclarationException>(() => UseCaseCatalog.FromAssemblies([typeof(CatalogSamples).Assembly]));

        thrown.Problems.ShouldContain(p => p.Contains(nameof(CatalogSamples.NoDeclarationCommand), StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains(nameof(CatalogSamples.NotAHandler), StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(typeof(LookupThingQuery))]
    [InlineData(typeof(CatalogSamples.ReferenceOfDictionaryQuery))]
    [InlineData(typeof(CatalogSamples.ReferenceOfArrayQuery))]
    [InlineData(typeof(CatalogSamples.ReferenceOfListQuery))]
    [InlineData(typeof(CatalogSamples.ReferenceExemptQuery))]
    [InlineData(typeof(CatalogSamples.CallerScopedStringQuery))]
    public void Build_ReferenceQueryWithAllowedResult_Accepted(Type query) =>
        Should.NotThrow(() => UseCaseCatalog.Build([query]));

    [Theory]
    [InlineData(typeof(CatalogSamples.ReferenceOfStringQuery), "只能是 ReferenceInfo 或它的集合、字典，现在是 String")]
    [InlineData(typeof(CatalogSamples.ReferenceOfNamesQuery), "现在是 IReadOnlyDictionary`2")]
    [InlineData(typeof(CatalogSamples.ReferenceOfWrongKeyQuery), "现在是 IReadOnlyDictionary`2")]
    [InlineData(typeof(CatalogSamples.ReferenceUselessExemptionQuery), "不需要 ResultExemption")]
    [InlineData(typeof(CatalogSamples.ReferenceBlankExemptionQuery), "ResultExemption 必须写明原因")]
    [InlineData(typeof(CatalogSamples.CallerScopedExemptionQuery), "ResultExemption 只能用在")]
    public void Build_ReferenceQueryWithWrongResult_RefusesWithReason(Type query, string reason)
    {
        var thrown = Should.Throw<DeclarationException>(() => UseCaseCatalog.Build([query]));

        thrown.Problems.ShouldContain(p => p.Contains(reason, StringComparison.Ordinal) && p.Contains(query.Name, StringComparison.Ordinal));
    }
}
