using System.Reflection;

namespace Kmic.ArchitectureTests.Support;

/// <summary>
/// 一个模块的某一层程序集，按名字 Kmic.{模块}.{层} 识别
/// </summary>
internal sealed record ProductAssembly(Assembly Assembly, string Module, Layer Layer)
{
    public string Name => Assembly.GetName().Name!;

    public bool IsModule => !string.Equals(Module, ProductAssemblies.FrameworkModule, StringComparison.Ordinal);
}
