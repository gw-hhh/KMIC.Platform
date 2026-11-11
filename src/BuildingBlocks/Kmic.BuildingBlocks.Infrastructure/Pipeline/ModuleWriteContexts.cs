using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 模块名到写入上下文的登记；事务行为按用例所属模块取得要开启事务的上下文
/// </summary>
internal sealed class ModuleWriteContexts(IEnumerable<ModuleWriteContext> registrations, IServiceProvider services)
{
    private readonly Dictionary<string, Type> _byModule = registrations.ToDictionary(r => r.Module, r => r.ContextType, StringComparer.Ordinal);

    public ModuleDbContext Get(string module) =>
        _byModule.TryGetValue(module, out var type)
            ? (ModuleDbContext)services.GetRequiredService(type)
            : throw new InvalidOperationException($"{module} 模块没有注册写入上下文（AddModuleDbContexts），它的命令无法开启事务。");
}
