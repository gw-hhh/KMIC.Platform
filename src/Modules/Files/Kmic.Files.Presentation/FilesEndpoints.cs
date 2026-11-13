using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Files.Contracts;

namespace Kmic.Files.Presentation;

/// <summary>
/// Files 模块的端点注册类
/// </summary>
public sealed class FilesEndpoints : ModuleEndpoints
{
    /// <inheritdoc />
    public override string Module => FilesModuleInfo.Name;
}
