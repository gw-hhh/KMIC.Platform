using Kmic.Configuration.Contracts;
using Shouldly;

namespace Kmic.Configuration.Domain.Tests;

public sealed class ModuleInfoTests
{
    [Fact]
    public void ModuleName_IsStable() => ConfigurationModuleInfo.Name.ShouldBe("Configuration");
}
