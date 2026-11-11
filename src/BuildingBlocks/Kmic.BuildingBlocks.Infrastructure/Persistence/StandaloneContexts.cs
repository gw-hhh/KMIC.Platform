using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在 DI 之外创建模块上下文：设计时工具（dotnet ef）与迁移程序用，身份为启动任务的系统身份
/// </summary>
internal static class StandaloneContexts
{
    public static TContext Create<TContext>(string connectionString)
        where TContext : ModuleDbContext
    {
        var options = new DbContextOptionsBuilder<TContext>().UseNpgsql(connectionString).Options;
        return (TContext)Activator.CreateInstance(typeof(TContext), options, Services(typeof(TContext)))!;
    }

    // 模型校验要核对范围对象：从上下文所在程序集及其引用的 Kmic.* 程序集中读取声明
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
