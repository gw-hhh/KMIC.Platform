using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在依赖注入之外创建模块上下文
/// </summary>
/// <remarks>
/// 给 dotnet ef、迁移程序和运维命令用，身份是系统身份
/// </remarks>
public static class StandaloneContexts
{
    /// <summary>
    /// 用连接串直接创建模块的写入上下文
    /// </summary>
    /// <remarks>
    /// 不走依赖注入，也不走命令管道：保存要用 <see cref="ModuleDbContext.SaveForToolAsync" />。只给运维程序和设计时工具用，业务代码不能用
    /// </remarks>
    public static TContext Create<TContext>(string connectionString)
        where TContext : ModuleDbContext
    {
        var options = new DbContextOptionsBuilder<TContext>().UseNpgsql(connectionString).Options;
        return (TContext)Activator.CreateInstance(typeof(TContext), options, Services(typeof(TContext)))!;
    }

    // 模型校验要核对范围对象，从上下文所在程序集及其引用的 Kmic.* 程序集里读取
    private static ModuleDbContextServices Services(Type contextType)
    {
        var execution = new ExecutionScope();
        execution.Initialize(CallOrigin.Startup, CurrentUser.System());
        var assemblies = contextType.Assembly.GetReferencedAssemblies()
            .Where(name => name.Name?.StartsWith("Kmic.", StringComparison.Ordinal) == true)
            .Select(Assembly.Load)
            .Prepend(contextType.Assembly);
        return new ModuleDbContextServices(
            execution,
            new UnrestrictedDataScope(),
            DataScopeObjectCatalog.FromAssemblies(assemblies),
            TimeProvider.System,
            new SaveChangesGate(),
            new NoSecurityEvents());
    }

    private sealed class UnrestrictedDataScope : IDataScope
    {
        public bool Unrestricted => true;

        public EffectiveScope For(string scopeObject) => EffectiveScope.Unrestricted;
    }

    private sealed class NoSecurityEvents : ISecurityEventWriter
    {
        public void Write(SecurityEvent securityEvent) =>
            throw new InvalidOperationException("迁移与设计时工具不写安全事件。");
    }
}
