using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 各模块的框架表名
/// </summary>
/// <remarks>
/// 投递程序要写别的模块的 Inbox，这是框架基础设施的例外，模块代码不能这样做
/// </remarks>
internal sealed class ModuleFrameworkTables(IEnumerable<ModuleWriteContext> registrations, IExecutionScopeFactory scopes)
{
    private readonly Lazy<FrozenDictionary<string, FrameworkTableNames>> _byModule = new(() => Load(registrations, scopes));

    public IEnumerable<string> Modules => _byModule.Value.Keys;

    public bool Contains(string module) => _byModule.Value.ContainsKey(module);

    public FrameworkTableNames For(string module) =>
        _byModule.Value.GetValueOrDefault(module)
            ?? throw new InvalidOperationException($"{module} 模块没有注册写入上下文，请调用 AddModuleDbContexts。");

    // 表名从模型里读，和迁移建出来的一致；建上下文要先有执行上下文，用系统身份
    private static FrozenDictionary<string, FrameworkTableNames> Load(IEnumerable<ModuleWriteContext> registrations, IExecutionScopeFactory scopes)
    {
        using var scope = scopes.CreateScope(CallOrigin.Inbox, CurrentUser.System());
        return registrations.ToFrozenDictionary(
            r => r.Module,
            r =>
            {
                var model = ((ModuleDbContext)scope.ServiceProvider.GetRequiredService(r.ContextType)).Model;
                return new FrameworkTableNames(
                    TableOf(model, typeof(OutboxMessage)), TableOf(model, typeof(InboxMessage)), TableOf(model, typeof(BackgroundJobRecord)));
            },
            StringComparer.Ordinal);
    }

    private static string TableOf(IModel model, Type entity)
    {
        var type = model.FindEntityType(entity) ?? throw new InvalidOperationException($"模型中没有 {entity.Name}。");
        return $"\"{type.GetSchema()}\".\"{type.GetTableName()}\"";
    }
}
