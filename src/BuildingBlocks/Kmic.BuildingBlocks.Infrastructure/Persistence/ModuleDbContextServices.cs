using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块上下文依赖的框架服务
/// </summary>
/// <remarks>
/// 合成一个参数，模块上下文的构造函数就不用逐个列出
/// </remarks>
/// <param name="execution">执行上下文</param>
/// <param name="dataScope">数据权限</param>
/// <param name="scopeObjects">范围对象目录</param>
/// <param name="time">时间来源</param>
/// <param name="saveGate">保存入口控制</param>
/// <param name="securityEvents">安全事件</param>
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
