using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Domain.Results;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Jobs;

public sealed class BackgroundJobCatalogTests
{
    [Fact]
    public void Build_Valid_DescribesJobAndPermission()
    {
        var catalog = BackgroundJobCatalog.Build([typeof(Export), typeof(ExportHandler)]);

        var job = catalog.Describe(typeof(Export));
        job.Name.ShouldBe("BuildingBlocks.Export");
        job.HandlerType.ShouldBe(typeof(ExportHandler));
        job.HandlerInterface.ShouldBe(typeof(IJobHandler<Export>));
        catalog.PermissionReferences.ShouldHaveSingleItem().Permission.ShouldBe("BuildingBlocks.Thing.Export");
        catalog.Find("BuildingBlocks.Export", 1).ShouldBe(job);
        catalog.DisplayNameOf("BuildingBlocks.Export").ShouldBe("导出");
        catalog.DisplayNameOf("BuildingBlocks.Retired").ShouldBe("BuildingBlocks.Retired", "代码里已没有的作业显示作业类型名");
    }

    [Fact]
    public void Build_BlankDisplayName_Rejected()
    {
        var thrown = Should.Throw<DeclarationException>(() => BackgroundJobCatalog.Build([typeof(Unlabelled), typeof(UnlabelledHandler)]));

        thrown.Problems.ShouldHaveSingleItem().ShouldContain("显示名称");
    }

    [Fact]
    public void FromAssemblies_AlwaysIncludesFrameworkRebuildJob()
    {
        var job = BackgroundJobCatalog.FromAssemblies([]).Describe(typeof(ReadModelRebuildJob));

        job.Name.ShouldBe("BuildingBlocks.RebuildReadModel");
        job.Attribute.SystemOnly.ShouldBeTrue();
        job.HandlerType.ShouldBeNull();
    }


    [Fact]
    public void Build_ExportJobNotAudited_Rejected()
    {
        var thrown = Should.Throw<DeclarationException>(() => BackgroundJobCatalog.Build([typeof(UnauditedExport), typeof(UnauditedExportHandler)]));

        thrown.Problems.ShouldHaveSingleItem().ShouldContain("Audited = true");
    }
    [Fact]
    public void Build_WrongDeclarations_ListsEveryProblem()
    {
        var thrown = Should.Throw<DeclarationException>(() => BackgroundJobCatalog.Build(
            [typeof(Unnamed), typeof(WrongPrefix), typeof(NeitherPermissionNorSystem), typeof(BothPermissionAndSystem), typeof(NoHandler),
             typeof(TwoHandlers), typeof(TwoHandlersA), typeof(TwoHandlersB), typeof(WrongPrefixHandler), typeof(NeitherHandler), typeof(BothHandler)]));

        thrown.Problems.ShouldContain(p => p.Contains(nameof(Unnamed), StringComparison.Ordinal) && p.Contains("[BackgroundJob]", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Sales.WrongPrefix", StringComparison.Ordinal) && p.Contains("BuildingBlocks.", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.Neither", StringComparison.Ordinal) && p.Contains("只能选一个", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.Both", StringComparison.Ordinal) && p.Contains("只能选一个", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.NoHandler", StringComparison.Ordinal) && p.Contains("0 个", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.TwoHandlers", StringComparison.Ordinal) && p.Contains("2 个", StringComparison.Ordinal));
        thrown.Problems.Count.ShouldBe(6);
    }

    [BackgroundJob("BuildingBlocks.Export", "导出", Permission = "BuildingBlocks.Thing.Export", Audited = true)]
    private sealed record Export : IBackgroundJob;


    [BackgroundJob("BuildingBlocks.ExportAll", "全部导出", Permission = "BuildingBlocks.Thing.Export")]
    private sealed record UnauditedExport : IBackgroundJob;

    private sealed class UnauditedExportHandler : IJobHandler<UnauditedExport>
    {
        public Task<Result<JobOutcome>> ExecuteAsync(UnauditedExport job, IJobContext context, CancellationToken ct) => Task.FromResult<Result<JobOutcome>>(new JobOutcome());
    }
    private sealed record Unnamed : IBackgroundJob;

    [BackgroundJob("BuildingBlocks.Unlabelled", " ", SystemOnly = true)]
    private sealed record Unlabelled : IBackgroundJob;

    private sealed class UnlabelledHandler : IJobHandler<Unlabelled>
    {
        public Task<Result<JobOutcome>> ExecuteAsync(Unlabelled job, IJobContext context, CancellationToken ct) => Task.FromResult<Result<JobOutcome>>(new JobOutcome());
    }

    [BackgroundJob("Sales.WrongPrefix", "前缀不对", SystemOnly = true)]
    private sealed record WrongPrefix : IBackgroundJob;

    [BackgroundJob("BuildingBlocks.Neither", "两个都没写")]
    private sealed record NeitherPermissionNorSystem : IBackgroundJob;

    [BackgroundJob("BuildingBlocks.Both", "两个都写", Permission = "BuildingBlocks.Thing.Export", SystemOnly = true)]
    private sealed record BothPermissionAndSystem : IBackgroundJob;

    [BackgroundJob("BuildingBlocks.NoHandler", "没有处理器", SystemOnly = true)]
    private sealed record NoHandler : IBackgroundJob;

    [BackgroundJob("BuildingBlocks.TwoHandlers", "两个处理器", SystemOnly = true)]
    private sealed record TwoHandlers : IBackgroundJob;

    private sealed class ExportHandler : IJobHandler<Export>
    {
        public Task<Result<JobOutcome>> ExecuteAsync(Export job, IJobContext context, CancellationToken ct) => Task.FromResult<Result<JobOutcome>>(new JobOutcome());
    }

    private sealed class TwoHandlersA : IJobHandler<TwoHandlers>
    {
        public Task<Result<JobOutcome>> ExecuteAsync(TwoHandlers job, IJobContext context, CancellationToken ct) => Task.FromResult<Result<JobOutcome>>(new JobOutcome());
    }

    private sealed class TwoHandlersB : IJobHandler<TwoHandlers>
    {
        public Task<Result<JobOutcome>> ExecuteAsync(TwoHandlers job, IJobContext context, CancellationToken ct) => Task.FromResult<Result<JobOutcome>>(new JobOutcome());
    }

    private sealed class WrongPrefixHandler : IJobHandler<WrongPrefix>
    {
        public Task<Result<JobOutcome>> ExecuteAsync(WrongPrefix job, IJobContext context, CancellationToken ct) => Task.FromResult<Result<JobOutcome>>(new JobOutcome());
    }

    private sealed class NeitherHandler : IJobHandler<NeitherPermissionNorSystem>
    {
        public Task<Result<JobOutcome>> ExecuteAsync(NeitherPermissionNorSystem job, IJobContext context, CancellationToken ct) =>
            Task.FromResult<Result<JobOutcome>>(new JobOutcome());
    }

    private sealed class BothHandler : IJobHandler<BothPermissionAndSystem>
    {
        public Task<Result<JobOutcome>> ExecuteAsync(BothPermissionAndSystem job, IJobContext context, CancellationToken ct) =>
            Task.FromResult<Result<JobOutcome>>(new JobOutcome());
    }
}
