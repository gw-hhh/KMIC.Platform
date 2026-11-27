using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;

namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

// 在独立事务中将审计事件写入 Outbox（供敏感查询与作业结束复用）
internal sealed class ReadAuditWriter(AuditRecorder recorder, IntegrationMessageCatalog messages, ModuleWriteContexts contexts, OutboxWriter outbox)
{
    public Task WriteAsync(UseCase useCase, CancellationToken ct) =>
        WriteAsync(recorder.BuildForRead(useCase), useCase.Module, ct);

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
