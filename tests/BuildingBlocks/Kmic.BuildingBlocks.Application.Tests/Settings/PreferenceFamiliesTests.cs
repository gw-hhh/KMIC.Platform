using Kmic.BuildingBlocks.Application.Settings;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Settings;

public sealed class PreferenceFamiliesTests
{
    [Theory]
    [InlineData("Ui.Shell", "Ui.Shell")]
    [InlineData("Ui.Table.Sample.Specimens.Main", "Ui.Table")]
    [InlineData("Ui.Table.Identity.Users.List2", "Ui.Table")]
    public void Find_KeysInAFamily_ReturnTheFamily(string key, string family) =>
        PreferenceFamilies.Find(key).ShouldNotBeNull().Name.ShouldBe(family);

    [Theory]
    [InlineData("Ui.Shell.Dark")]
    [InlineData("Ui.Shell ")]
    [InlineData("ui.shell")]
    [InlineData("Ui.Table")]
    [InlineData("Ui.Table.Sample.Specimens")]
    [InlineData("Ui.Table.Sample.Specimens.Main.Extra")]
    [InlineData("Ui.Table.Sample.Home-Page.Main")]
    [InlineData("Ui.Table.Sample..Main")]
    [InlineData("Ui.Table.Sample.Specimens.主表")]
    [InlineData("Sample.List.PageSize")]
    [InlineData("")]
    public void Find_KeysOutsideEveryFamily_ReturnNothing(string key) =>
        PreferenceFamilies.Find(key).ShouldBeNull();

    [Fact]
    public void Find_KeyLongerThanTheLimit_ReturnsNothing()
    {
        var segment = new string('a', 50);

        PreferenceFamilies.Find($"Ui.Table.{segment}.{segment}.{segment}").ShouldBeNull();
        PreferenceFamilies.Find($"Ui.Table.{new string('a', 30)}.{new string('b', 30)}.{new string('c', 30)}").ShouldNotBeNull();
    }
}
