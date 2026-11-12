using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Operations.Contracts;

namespace Kmic.Operations.Presentation;

/// <summary>
/// Operations 模块的端点注册类
/// </summary>
public sealed class OperationsEndpoints : ModuleEndpoints
{
    /// <inheritdoc />
    public override string Module => OperationsModuleInfo.Name;
}
