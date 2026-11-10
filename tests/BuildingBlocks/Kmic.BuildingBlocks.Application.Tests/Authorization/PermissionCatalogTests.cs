using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Results;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Authorization;

public sealed class PermissionCatalogTests
{
    [Fact]
    public void Build_ValidGroup_ReadsDefinitions()
    {
        var catalog = PermissionCatalog.Build([typeof(GoodPermissions)]);

        catalog.All.ShouldBe(
        [
            new PermissionDefinition(GoodPermissions.View, "Thing", "东西", "查看东西", Sensitive: false),
            new PermissionDefinition(GoodPermissions.Purge, "Thing", "东西", "物理删除东西", Sensitive: true),
        ],
        ignoreOrder: true);
    }

    [Fact]
    public void Build_WrongCodes_ReportsEachProblem()
    {
        var thrown = Should.Throw<DeclarationException>(() => PermissionCatalog.Build([typeof(BadPermissions), typeof(GoodPermissions), typeof(DuplicatePermissions)]));

        thrown.Problems.ShouldContain(p => p.Contains("thing.Bad.View", StringComparison.Ordinal) && p.Contains("格式", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Other.Thing.View", StringComparison.Ordinal) && p.Contains("本模块名", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("NotConstant", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Thing.Thing.View 重复", StringComparison.Ordinal));
    }

    [Fact]
    public void Verify_ReferencesUnusedAndSensitive()
    {
        var catalog = PermissionCatalog.Build([typeof(GoodPermissions)]);
        var useCases = UseCaseCatalog.Build([typeof(PurgeThingCommand), typeof(ExportAllThingsQuery), typeof(UnknownPermissionQuery)]);

        var thrown = Should.Throw<DeclarationException>(() => catalog.Verify(useCases, [new PermissionReference("Thing.Thing.Gone", "资源动作 GadgetActions")]));

        thrown.Problems.ShouldContain(p => p.Contains("PurgeThingCommand", StringComparison.Ordinal) && p.Contains("必须 [Audited]", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("ExportAllThingsQuery", StringComparison.Ordinal) && p.Contains("必须 [AuditedRead]", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("资源动作 GadgetActions：引用的权限点 Thing.Thing.Gone 不存在", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Thing.Thing.Missing 不存在", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Thing.Thing.View 没有被任何用例或资源动作使用", StringComparison.Ordinal));
    }

    [Fact]
    public void Verify_PermissionReferencedOutsideUseCases_Passes()
    {
        var catalog = PermissionCatalog.Build([typeof(GoodPermissions)]);
        var useCases = UseCaseCatalog.Build([typeof(AuditedPurgeThingCommand)]);

        Should.NotThrow(() => catalog.Verify(useCases, [new PermissionReference(GoodPermissions.View, "系统设置 Thing.Display.Mode")]));
    }

    [PermissionGroup("Thing", "东西")]
    public static class GoodPermissions
    {
        [Permission("查看东西")]
        public const string View = "Thing.Thing.View";

        [Permission("物理删除东西", Sensitive = true)]
        public const string Purge = "Thing.Thing.Purge";
    }

    [PermissionGroup("Thing", "东西（错误示例）")]
    public static class BadPermissions
    {
        [Permission("格式不对")]
        public const string WrongFormat = "thing.Bad.View";

        [Permission("别的模块")]
        public const string OtherModule = "Other.Thing.View";

        public static readonly string NotConstant = "Thing.Bad.Edit";
    }

    [PermissionGroup("Thing", "重复")]
    public static class DuplicatePermissions
    {
        [Permission("又一个查看")]
        public const string View = "Thing.Thing.View";
    }

    [RequirePermission(GoodPermissions.Purge)]
    [NotAudited("错误示例：敏感权限点必须审计")]
    public sealed record PurgeThingCommand : ICommand;

    [RequirePermission(GoodPermissions.Purge)]
    [Audited]
    public sealed record AuditedPurgeThingCommand : ICommand;

    [RequirePermission(GoodPermissions.Purge)]
    public sealed record ExportAllThingsQuery : IQuery<int>;

    [RequirePermission("Thing.Thing.Missing")]
    public sealed record UnknownPermissionQuery : IQuery<int>;

    public sealed class PurgeThingCommandHandler : ICommandHandler<PurgeThingCommand>
    {
        public ValueTask<Result> Handle(PurgeThingCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    public sealed class AuditedPurgeThingCommandHandler : ICommandHandler<AuditedPurgeThingCommand>
    {
        public ValueTask<Result> Handle(AuditedPurgeThingCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    public sealed class ExportAllThingsQueryHandler : IQueryHandler<ExportAllThingsQuery, int>
    {
        public ValueTask<Result<int>> Handle(ExportAllThingsQuery query, CancellationToken cancellationToken) => ValueTask.FromResult<Result<int>>(0);
    }

    public sealed class UnknownPermissionQueryHandler : IQueryHandler<UnknownPermissionQuery, int>
    {
        public ValueTask<Result<int>> Handle(UnknownPermissionQuery query, CancellationToken cancellationToken) => ValueTask.FromResult<Result<int>>(0);
    }
}
