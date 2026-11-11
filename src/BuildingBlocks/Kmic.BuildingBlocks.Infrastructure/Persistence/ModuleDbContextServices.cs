using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块上下文依赖的框架服务，合成一个参数，模块上下文的构造函数不必逐个列出
/// </summary>
/// <param name="execution">执行上下文：审计列的操作人</param>
/// <param name="dataScope">当前请求的数据范围，由命名过滤器读取</param>
/// <param name="scopeObjects">范围对象目录，构建模型时核对 HasDataScope 引用的编码</param>
/// <param name="time">时间来源</param>
/// <param name="saveGate">保存守卫</param>
/// <param name="securityEvents">安全事件：仓储加载到范围外的聚合时记录真实原因</param>
public sealed class ModuleDbContextServices(
    IExecutionContext execution,
    IDataScope dataScope,
    DataScopeObjectCatalog scopeObjects,
    TimeProvider time,
    SaveChangesGate saveGate,
    ISecurityEventWriter securityEvents)
{
    internal IExecutionContext Execution { get; } = execution ?? throw new ArgumentNullException(nameof(execution));

    internal IDataScope DataScope { get; } = dataScope ?? throw new ArgumentNullException(nameof(dataScope));

    internal DataScopeObjectCatalog ScopeObjects { get; } = scopeObjects ?? throw new ArgumentNullException(nameof(scopeObjects));

    internal TimeProvider Time { get; } = time ?? throw new ArgumentNullException(nameof(time));

    internal SaveChangesGate SaveGate { get; } = saveGate ?? throw new ArgumentNullException(nameof(saveGate));

    internal ISecurityEventWriter SecurityEvents { get; } = securityEvents ?? throw new ArgumentNullException(nameof(securityEvents));
}
