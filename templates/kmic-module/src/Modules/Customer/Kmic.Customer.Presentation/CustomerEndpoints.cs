using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Customer.Contracts;

namespace Kmic.Customer.Presentation;

/// <summary>
/// Customer 模块的端点注册类
/// </summary>
public sealed class CustomerEndpoints : ModuleEndpoints
{
    /// <inheritdoc />
    public override string Module => CustomerModuleInfo.Name;
}
