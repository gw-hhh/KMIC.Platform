using Kmic.Operations.Contracts;
using Shouldly;

namespace Kmic.Operations.Domain.Tests;

public sealed class ModuleInfoTests
{
    [Fact]
    public void ModuleName_IsStable() => OperationsModuleInfo.Name.ShouldBe("Operations");
}
