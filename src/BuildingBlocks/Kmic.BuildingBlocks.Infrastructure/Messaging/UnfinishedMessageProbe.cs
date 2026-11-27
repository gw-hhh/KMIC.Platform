using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal sealed class UnfinishedMessageProbe(IExecutionContext execution, CommandTransaction transaction, ModuleWriteContexts contexts, ModuleFrameworkTables tables) : IUnfinishedMessageProbe
{
    public async Task<IReadOnlySet<Guid>> FindSubjectsAsync(string messageType, IReadOnlyCollection<Guid> subjectIds, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentNullException.ThrowIfNull(subjectIds);
        if (!execution.InCommand || transaction.Module is not { } module)
        {
            throw new InvalidOperationException("探查未完成消息仅限在命令事务内调用（需受事务锁保护）。");
        }

        var found = new HashSet<Guid>();
        if (subjectIds.Count == 0)
        {
            return found;
        }

        var database = contexts.Get(module).Database;
        var ids = subjectIds.Distinct().ToArray();
        var modules = tables.Modules.Order(StringComparer.Ordinal).ToList();

        // 须严格先查 Outbox 再查 Inbox，避免消息在查询间隙刚完成投递而漏检
        foreach (var table in modules.Select(m => tables.For(m).Outbox))
        {
            found.UnionWith(await QueryAsync(database, table, messageType, ids, ct));
        }

        foreach (var table in modules.Select(m => tables.For(m).Inbox))
        {
            found.UnionWith(await QueryAsync(database, table, messageType, ids, ct));
        }

        return found;
    }

    // 待投递/待处理、处理中与死信均计为未完成；已完成和已放弃的不计入
    private static async Task<List<Guid>> QueryAsync(DatabaseFacade database, string table, string messageType, Guid[] ids, CancellationToken ct)
    {
        // 表名来自各模块模型，不含外部输入；参数值均通过参数化传递
        var sql = $"""
            SELECT DISTINCT s AS "Value"
            FROM {table} m CROSS JOIN LATERAL unnest(m."SubjectIds") AS s
            WHERE m."MessageType" = @type AND m."Status" IN ({MessageStatus.Pending}, {MessageStatus.Processing}, {MessageStatus.Dead})
              AND m."SubjectIds" && @ids AND s = ANY(@ids)
            """;
        return await database.SqlQueryRaw<Guid>(
            sql,
            new NpgsqlParameter("type", messageType),
            new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = ids })
            .ToListAsync(ct);
    }
}
