using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed class ModuleFrameworkTables(IEnumerable<ModuleWriteContext> registrations, IExecutionScopeFactory scopes)
{
    private readonly Lazy<FrozenDictionary<string, FrameworkTableNames>> _byModule = new(() => Load(registrations, scopes));

    public IEnumerable<string> Modules => _byModule.Value.Keys;

    public bool Contains(string module) => _byModule.Value.ContainsKey(module);

    public FrameworkTableNames For(string module) =>
        _byModule.Value.GetValueOrDefault(module)
            ?? throw new InvalidOperationException($"{module} 模块没有注册写入上下文，请调用 AddModuleDbContexts。");

    // 直接从 EF 模型读取表名以确保与迁移结果一致；实例化上下文前须设置系统身份的执行上下文
    private static FrozenDictionary<string, FrameworkTableNames> Load(IEnumerable<ModuleWriteContext> registrations, IExecutionScopeFactory scopes)
    {
        using var scope = scopes.CreateScope(CallOrigin.Inbox, CurrentUser.System());
        return registrations.ToFrozenDictionary(
            r => r.Module,
            r =>
            {
                var model = ((ModuleDbContext)scope.ServiceProvider.GetRequiredService(r.ContextType)).Model;
                return new FrameworkTableNames(
                    TableOf(model, typeof(IdempotencyRecord)),
                    TableOf(model, typeof(OutboxMessage)), TableOf(model, typeof(InboxMessage)), TableOf(model, typeof(BackgroundJobRecord)),
                    TableOf(model, typeof(ReadModelRebuildRun)), TableOf(model, typeof(NumberCounterRecord)));
            },
            StringComparer.Ordinal);
    }

    private static string TableOf(IModel model, Type entity)
    {
        var type = model.FindEntityType(entity) ?? throw new InvalidOperationException($"模型中没有 {entity.Name}。");
        return $"\"{type.GetSchema()}\".\"{type.GetTableName()}\"";
    }
}
