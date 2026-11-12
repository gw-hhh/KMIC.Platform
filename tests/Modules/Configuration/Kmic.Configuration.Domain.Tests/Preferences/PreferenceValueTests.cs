using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Configuration.Domain.Preferences;
using Shouldly;

namespace Kmic.Configuration.Domain.Tests.Preferences;

public sealed class PreferenceValueTests
{
    private const string Json = """{"value":30}""";

    [Fact]
    public void ForSystem_HasNeitherRoleNorUser()
    {
        var value = PreferenceValue.ForSystem("Sample.List.PageSize", Json);

        value.Layer.ShouldBe(PreferenceLayer.System);
        value.RoleId.ShouldBeNull();
        value.UserId.ShouldBeNull();
        value.Value.ShouldBe(Json);
    }

    [Fact]
    public void ForRole_HasARoleButNoUser()
    {
        var role = RoleId.New();

        var value = PreferenceValue.ForRole("Sample.List.PageSize", role, Json);

        value.Layer.ShouldBe(PreferenceLayer.Role);
        value.RoleId.ShouldBe(role);
        value.UserId.ShouldBeNull();
    }

    [Fact]
    public void ForUserRole_HasBothTheRoleAndTheUser()
    {
        var role = RoleId.New();
        var user = UserId.New();

        var value = PreferenceValue.ForUserRole("Sample.List.PageSize", role, user, Json);

        value.Layer.ShouldBe(PreferenceLayer.UserRole);
        value.RoleId.ShouldBe(role);
        value.UserId.ShouldBe(user);
    }

    [Fact]
    public void Set_ReplacesTheValue_AndNothingElse()
    {
        var value = PreferenceValue.ForSystem("Sample.List.PageSize", Json);
        var id = value.Id;

        value.Set("""{"value":80}""");

        value.Value.ShouldBe("""{"value":80}""");
        value.Id.ShouldBe(id);
        value.Key.ShouldBe("Sample.List.PageSize");
        value.Layer.ShouldBe(PreferenceLayer.System);
    }

    [Fact]
    public void AKeyOfExactlyTheLimit_IsAccepted_AndOneMoreIsRefused()
    {
        PreferenceValue.ForSystem(new string('A', PreferenceValue.MaxKeyLength), Json).ShouldNotBeNull();

        Should.Throw<ArgumentOutOfRangeException>(() => PreferenceValue.ForSystem(new string('A', PreferenceValue.MaxKeyLength + 1), Json));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ABlankKeyOrValue_IsRefused(string blank)
    {
        Should.Throw<ArgumentException>(() => PreferenceValue.ForSystem(blank, Json));
        Should.Throw<ArgumentException>(() => PreferenceValue.ForSystem("Sample.List.PageSize", blank));
        Should.Throw<ArgumentException>(() => PreferenceValue.ForSystem("Sample.List.PageSize", Json).Set(blank));
    }
}
