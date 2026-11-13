using Kmic.Notifications.Contracts;
using Shouldly;

namespace Kmic.Notifications.Domain.Tests;

public sealed class ModuleInfoTests
{
    [Fact]
    public void ModuleName_IsStable() => NotificationsModuleInfo.Name.ShouldBe("Notifications");
}
