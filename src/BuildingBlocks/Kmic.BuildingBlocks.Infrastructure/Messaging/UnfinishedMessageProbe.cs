using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// <see cref="IUnfinishedMessageProbe" /> 的实现：在当前命令的事务连接上查各模块的发件箱与收件箱
/// </summary>
/// <remarks>
/// 同一个数据库里的不同 schema 可以用一个连接查，所以不另借连接，也就不会在连接池紧张时和自己的事务互相等。
/// 先查全部发件箱再查全部收件箱，顺序见接口说明，不能打乱
/// </remarks>
internal sealed class UnfinishedMessageProbe(IExecutionContext execution, CommandTransaction transaction, ModuleWriteContexts contexts, ModuleFrameworkTables tables) : IUnfinishedMessageProbe
{
    public async Task<IReadOnlySet<Guid>> FindSubjectsAsync(string messageType, IReadOnlyCollection<Guid> subjectIds, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentNullException.ThrowIfNull(subjectIds);
        if (!execution.InCommand || transaction.Module is not { } module)
        {
            throw new InvalidOperationException("探查未完成的消息只能在命令里做：要在命令事务的锁保护之下读。");
        }

        var found = new HashSet<Guid>();
        if (subjectIds.Count == 0)
        {
            return found;
        }

        var database = contexts.Get(module).Database;
        var ids = subjectIds.Distinct().ToArray();
        var modules = tables.Modules.Order(StringComparer.Ordinal).ToList();
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

    // 待投递 / 待处理、处理中、死信都算未完成；已完成和已放弃的不算
    private static async Task<List<Guid>> QueryAsync(DatabaseFacade database, string table, string messageType, Guid[] ids, CancellationToken ct)
    {
        // 表名来自各模块的模型，不含外部输入；值都走参数
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
