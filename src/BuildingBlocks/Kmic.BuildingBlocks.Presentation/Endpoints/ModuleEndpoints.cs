namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 模块的端点注册类
/// </summary>
/// <remarks>
/// 路由前缀是 /api/v1/{模块名小写，多个单词用连字符}
/// </remarks>
public abstract class ModuleEndpoints
{
    /// <summary>
    /// 模块名，与 Infrastructure 里模块注册类的名字一致
    /// </summary>
    public abstract string Module { get; }

    /// <summary>
    /// 模块级的路由组设置，极少使用
    /// </summary>
    public virtual void ConfigureGroup(RouteGroupBuilder group)
    {
    }

    /// <summary>
    /// 本模块的端点类型，默认是子类所在程序集里的全部 <see cref="IEndpoint"/> 实现
    /// </summary>
    /// <remarks>
    /// 只有框架测试需要重写
    /// </remarks>
    protected virtual IEnumerable<Type> FindEndpointTypes() =>
        GetType().Assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t));

    // 按类型名排序，注册顺序不随反射的返回顺序变化
    internal IEnumerable<IEndpoint> CreateEndpoints() => FindEndpointTypes()
        .OrderBy(t => t.FullName, StringComparer.Ordinal)
        .Select(t => (IEndpoint)(Activator.CreateInstance(t, nonPublic: true) ?? throw new InvalidOperationException($"无法创建端点 {t.FullName}。")));
}
