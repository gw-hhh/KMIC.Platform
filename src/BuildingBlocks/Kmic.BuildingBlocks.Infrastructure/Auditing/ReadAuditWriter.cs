using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;

namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// 敏感查询成功之后写审计事件
/// </summary>
/// <remarks>
/// 查询没有事务，所以用一个独立的短事务把事件写进本模块的 Outbox。写入和查询不在同一个事务里：
/// 查询已经返回、审计却没写进去的情况只能记日志，所以读审计是尽力而为，只用在敏感查询上
/// </remarks>
internal sealed class ReadAuditWriter(AuditRecorder recorder, IntegrationMessageCatalog messages, ModuleWriteContexts contexts, OutboxWriter outbox)
{
    public Task WriteAsync(UseCase useCase, CancellationToken ct) =>
        WriteAsync(recorder.BuildForRead(useCase), useCase.Module, ct);

    /// <summary>
    /// 写一条已经生成好的审计事件，作业结束时的审计也用它
    /// </summary>
    public async Task WriteAsync(AuditRecorded audit, string module, CancellationToken ct)
    {
        var context = contexts.Get(module);
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        using (context.Services.SaveGate.Open())
        {
            await outbox.WriteAsync(context, module, [new PendingMessage(audit, messages.Describe(typeof(AuditRecorded)))], ct);
        }

        await CommandCommit.CommitAsync(transaction);
    }
}
