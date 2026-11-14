using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 各模块的查询上下文
/// </summary>
internal sealed class ModuleQueryContexts(IEnumerable<ModuleQueryContext> registrations, IServiceProvider services)
{
    private readonly Dictionary<string, Type> _byModule = registrations.ToDictionary(r => r.Module, r => r.ContextType, StringComparer.Ordinal);

    public ModuleQueryDbContext Get(string module) =>
        _byModule.TryGetValue(module, out var type)
            ? (ModuleQueryDbContext)services.GetRequiredService(type)
            : throw new InvalidOperationException($"{module} 模块没有注册查询上下文，请调用 AddModuleDbContexts。");
}
