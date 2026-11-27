using System.Reflection;

namespace Kmic.ArchitectureTests.Support;

// 按命名约定 Kmic.{模块}.{层} 识别的程序集
internal sealed record ProductAssembly(Assembly Assembly, string Module, Layer Layer)
{
    public string Name => Assembly.GetName().Name!;

    public bool IsModule => !string.Equals(Module, ProductAssemblies.FrameworkModule, StringComparison.Ordinal);
}
