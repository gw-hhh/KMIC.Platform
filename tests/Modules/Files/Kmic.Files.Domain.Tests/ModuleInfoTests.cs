using Kmic.Files.Contracts;
using Shouldly;

namespace Kmic.Files.Domain.Tests;

public sealed class ModuleInfoTests
{
    [Fact]
    public void ModuleName_IsStable() => FilesModuleInfo.Name.ShouldBe("Files");
}
