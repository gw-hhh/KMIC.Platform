using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

internal sealed class ModuleWriteContexts(IEnumerable<ModuleWriteContext> registrations, IServiceProvider services)
{
    private readonly Dictionary<string, Type> _byModule = registrations.ToDictionary(r => r.Module, r => r.ContextType, StringComparer.Ordinal);

    public ModuleDbContext Get(string module) =>
        _byModule.TryGetValue(module, out var type)
            ? (ModuleDbContext)services.GetRequiredService(type)
            : throw new InvalidOperationException($"{module} 模块未注册写入上下文，请先调用 AddModuleDbContexts。");
}
