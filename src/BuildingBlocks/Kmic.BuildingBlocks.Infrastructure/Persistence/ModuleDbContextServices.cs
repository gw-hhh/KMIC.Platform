using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>模块数据库上下文依赖的框架服务</summary>
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
