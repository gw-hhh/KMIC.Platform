using Kmic.BuildingBlocks.Presentation.Endpoints;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

/// <summary>
/// 只包含指定端点的模块：同一个测试程序集里要准备很多组互不相干的端点
/// </summary>
internal sealed class ListedModule(string module, params Type[] endpoints) : ModuleEndpoints
{
    public override string Module => module;

    protected override IEnumerable<Type> FindEndpointTypes() => endpoints;
}
