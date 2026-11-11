using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 各模块的写入上下文
/// </summary>
internal sealed class ModuleWriteContexts(IEnumerable<ModuleWriteContext> registrations, IServiceProvider services)
{
    private readonly Dictionary<string, Type> _byModule = registrations.ToDictionary(r => r.Module, r => r.ContextType, StringComparer.Ordinal);

    public ModuleDbContext Get(string module) =>
        _byModule.TryGetValue(module, out var type)
            ? (ModuleDbContext)services.GetRequiredService(type)
            : throw new InvalidOperationException($"{module} 模块没有注册写入上下文，请调用 AddModuleDbContexts。");
}
