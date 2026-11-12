using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Presentation;

/// <summary>
/// Identity 模块的端点注册类
/// </summary>
public sealed class IdentityEndpoints : ModuleEndpoints
{
    /// <inheritdoc />
    public override string Module => IdentityModuleInfo.Name;
}
