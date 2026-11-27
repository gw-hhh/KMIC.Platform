namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>模块端点注册基类（路由前缀固定为 /api/v1/{模块名小写连字符}）</summary>
public abstract class ModuleEndpoints
{
    /// <summary>模块名称，须与 Infrastructure 层模块注册名称一致</summary>
    public abstract string Module { get; }

    /// <summary>模块级路由组扩展配置</summary>
    public virtual void ConfigureGroup(RouteGroupBuilder group)
    {
    }

    /// <summary>发现当前模块的端点类型，默认扫描所在程序集全部 IEndpoint 实现（仅测试场景需重写）</summary>
    protected virtual IEnumerable<Type> FindEndpointTypes() =>
        GetType().Assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t));

    // 按类型全名排序，确保注册顺序稳定，不受反射枚举顺序影响
    internal IEnumerable<IEndpoint> CreateEndpoints() => FindEndpointTypes()
        .OrderBy(t => t.FullName, StringComparer.Ordinal)
        .Select(t => (IEndpoint)(Activator.CreateInstance(t, nonPublic: true) ?? throw new InvalidOperationException($"无法创建端点 {t.FullName}。")));
}
