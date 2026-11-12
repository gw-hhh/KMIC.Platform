using Kmic.Customer.Contracts;
using Shouldly;

namespace Kmic.Customer.Domain.Tests;

public sealed class ModuleInfoTests
{
    [Fact]
    public void ModuleName_IsStable() => CustomerModuleInfo.Name.ShouldBe("Customer");
}
