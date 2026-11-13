using Kmic.Operations.Domain.Jobs;
using Shouldly;

namespace Kmic.Operations.Domain.Tests.Jobs;

public sealed class ScheduledJobSettingTests
{
    [Fact]
    public void Create_KeepsOnlyWhatWasChanged()
    {
        var setting = ScheduledJobSetting.Create("Identity.PurgeSessions", enabled: false, cron: null);

        setting.JobName.ShouldBe("Identity.PurgeSessions");
        setting.Enabled.ShouldBe(false);
        setting.Cron.ShouldBeNull();
        setting.IsDefault.ShouldBeFalse();
    }

    [Fact]
    public void Apply_ReplacesBothItems_AndAnEmptyItemFollowsTheDefaultAgain()
    {
        var setting = ScheduledJobSetting.Create("Identity.PurgeSessions", enabled: false, cron: "0 15 4 * * ?");

        setting.Apply(enabled: null, cron: "  0 30 5 * * ?  ");

        setting.Enabled.ShouldBeNull();
        setting.Cron.ShouldBe("0 30 5 * * ?");
    }

    [Fact]
    public void RestoreDefault_ClearsBothItems_ButTheRowStays()
    {
        var setting = ScheduledJobSetting.Create("Identity.PurgeSessions", enabled: true, cron: "0 15 4 * * ?");

        setting.RestoreDefault();

        setting.IsDefault.ShouldBeTrue();
        setting.Enabled.ShouldBeNull();
        setting.Cron.ShouldBeNull();
        setting.JobName.ShouldBe("Identity.PurgeSessions");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ABlankJobName_IsRefused(string name) =>
        Should.Throw<ArgumentException>(() => ScheduledJobSetting.Create(name, enabled: true, cron: null));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankCron_IsRefused_NullMeansFollowTheDefault(string cron)
    {
        var setting = ScheduledJobSetting.Create("Identity.PurgeSessions", enabled: null, cron: null);

        Should.Throw<ArgumentException>(() => setting.Apply(enabled: null, cron));
    }

    [Fact]
    public void RunStatusAndTrigger_ReadTheCodesTheQueryProduces()
    {
        JobRunStatus.All.Select(s => s.Code).ShouldBe(["Running", "Succeeded", "Failed", "Unknown", "Abandoned"]);
        JobRunTrigger.All.Select(t => t.Code).ShouldBe(["Schedule", "Manual", "Misfire"]);
    }
}
