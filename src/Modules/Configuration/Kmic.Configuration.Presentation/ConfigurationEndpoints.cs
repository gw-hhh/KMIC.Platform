using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Presentation;

/// <summary>
/// Configuration 模块的端点注册类
/// </summary>
public sealed class ConfigurationEndpoints : ModuleEndpoints
{
    /// <inheritdoc />
    public override string Module => ConfigurationModuleInfo.Name;
}
