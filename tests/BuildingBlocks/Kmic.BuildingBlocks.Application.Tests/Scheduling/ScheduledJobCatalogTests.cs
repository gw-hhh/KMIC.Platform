using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Tests.Fixtures;
using Kmic.BuildingBlocks.Application.UseCases;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Scheduling;

public sealed class ScheduledJobCatalogTests
{
    // 测试程序集推出的模块名是 BuildingBlocks
    private const string Module = "BuildingBlocks";

    private static readonly UseCaseCatalog _useCases = UseCaseCatalog.Build([typeof(PurgeThingsCommand), typeof(RemindThingsCommand)]);

    [Fact]
    public void Build_ValidDeclaration_KeepsDefaultsAndOptions()
    {
        var module = new ModuleSchedules(Module);
        module.Schedule("BuildingBlocks.PurgeThings", "0 0 3 * * ?", run => new PurgeThingsCommand(run.ScheduledFireTime), o => o.OnMisfire(Misfire.Skip).DisplayName("清理"));
        module.Schedule("BuildingBlocks.PurgeAgain", "0 0 4 * * ?", run => new PurgeThingsCommand(run.ScheduledFireTime));

        var catalog = ScheduledJobCatalog.Build([module], _useCases, NoProblem);

        var job = catalog.Find("BuildingBlocks.PurgeThings").ShouldNotBeNull();
        job.Module.ShouldBe(Module);
        job.Cron.ShouldBe("0 0 3 * * ?");
        job.Misfire.ShouldBe(Misfire.Skip);
        job.DisplayName.ShouldBe("清理");
        var at = new DateTimeOffset(2026, 11, 10, 19, 0, 0, TimeSpan.Zero);
        job.Create(new ScheduledRun(at, ScheduledTrigger.Schedule)).ShouldBe(new PurgeThingsCommand(at));

        var other = catalog.Find("BuildingBlocks.PurgeAgain").ShouldNotBeNull();
        other.Misfire.ShouldBe(Misfire.FireOnceNow);
        other.DisplayName.ShouldBe("BuildingBlocks.PurgeAgain");
    }

    [Fact]
    public void Build_WrongDeclarations_ListsEveryProblem()
    {
        var module = new ModuleSchedules(Module);
        module.Schedule("Sales.PurgeThings", "0 0 3 * * ?", run => new PurgeThingsCommand(run.ScheduledFireTime));
        module.Schedule("BuildingBlocks.NotSystem", "0 0 3 * * ?", _ => new RemindThingsCommand());
        module.Schedule("BuildingBlocks.BadCron", "每天三点", run => new PurgeThingsCommand(run.ScheduledFireTime));
        module.Schedule("BuildingBlocks.Twice", "0 0 3 * * ?", run => new PurgeThingsCommand(run.ScheduledFireTime));
        module.Schedule("BuildingBlocks.Twice", "0 0 4 * * ?", run => new PurgeThingsCommand(run.ScheduledFireTime));

        var thrown = Should.Throw<DeclarationException>(() => ScheduledJobCatalog.Build([module], _useCases, c => c.StartsWith('0') ? null : "不是 Cron"));

        thrown.Problems.ShouldContain(p => p.Contains("Sales.PurgeThings", StringComparison.Ordinal) && p.Contains("BuildingBlocks.", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.NotSystem", StringComparison.Ordinal) && p.Contains("[SystemOnly]", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.BadCron", StringComparison.Ordinal) && p.Contains("不是 Cron", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.Twice", StringComparison.Ordinal) && p.Contains("2 次", StringComparison.Ordinal));
        thrown.Problems.Count.ShouldBe(4);
    }

    [Fact]
    public void Build_OtherModuleCommand_Rejected()
    {
        var module = new ModuleSchedules("Sales");
        module.Schedule("Sales.PurgeThings", "0 0 3 * * ?", run => new PurgeThingsCommand(run.ScheduledFireTime));

        var thrown = Should.Throw<DeclarationException>(() => ScheduledJobCatalog.Build([module], _useCases, NoProblem));

        thrown.Problems.ShouldHaveSingleItem().ShouldContain("要是 Sales 模块的命令");
    }

    private static string? NoProblem(string cron) => null;
}
