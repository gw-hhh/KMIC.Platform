using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 管道第 6 位（只对命令）：在用例所属模块的写入上下文上开启事务，处理器成功后分发领域事件、保存、提交
/// </summary>
/// <remarks>
/// 固定 Read Committed；处理器失败即回滚并清空变更跟踪器。提交本身不可取消：提交发出后再取消只会造成结果未知
/// </remarks>
public sealed class TransactionBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    IServiceProvider services,
    ILogger<TransactionBehavior<TMessage, TResponse>> logger) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseCommand
    where TResponse : Result, IResultFactory<TResponse>
{
    // 超过这么多轮仍有新事件，说明事件处理器之间互相触发
    private const int MaxDispatchRounds = 10;

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var context = services.GetRequiredService<ModuleWriteContexts>().Get(catalog.Get(typeof(TMessage)).Module);
        var transaction = services.GetRequiredService<CommandTransaction>();
        await using var dbTransaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            return await ExecuteAsync(message, next, context, transaction, cancellationToken);
        }
        catch
        {
            // 异常同样要丢弃本次的修改与挂接的动作；事务由 await using 回滚
            context.ChangeTracker.Clear();
            transaction.Discard();
            throw;
        }
    }

    private async ValueTask<TResponse> ExecuteAsync(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        ModuleDbContext context,
        CommandTransaction transaction,
        CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);
        if (response.IsSuccess && await DispatchDomainEventsAsync(context, cancellationToken) is { } eventFailure)
        {
            response = TResponse.Fail(eventFailure);
        }

        if (response.IsFailure)
        {
            await RollBackAsync(context, transaction);
            return response;
        }

        using (context.Services.SaveGate.Open())
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        response = WithSavedVersion(context, response);
        if (await transaction.RunBeforeCommitAsync(context, ResponseValues<TResponse>.Read(response), cancellationToken) is { } hookFailure)
        {
            await RollBackAsync(context, transaction);
            return TResponse.Fail(hookFailure);
        }

        await CommitAsync(context.Database.CurrentTransaction ?? throw new InvalidOperationException("事务在提交前已结束：处理器不能自己提交或回滚事务。"));
        await RunAfterCommitAsync(transaction.TakeAfterCommit());
        return response;
    }

    // 回填的是本次保存后的版本：版本在保存规则中递增，晚于处理器返回
    private static TResponse WithSavedVersion(ModuleDbContext context, TResponse response)
    {
        if (ResponseValues<TResponse>.Read(response) is not IHasConcurrencyVersion receipt)
        {
            return response;
        }

        var aggregate = context.ChangeTracker.Entries<IAggregateRoot>().FirstOrDefault(e =>
            e.Property(nameof(Entity<>.Id)).CurrentValue is IStronglyTypedId id && id.Value == receipt.AggregateId);
        return aggregate is null
            ? response
            : ResponseValues<TResponse>.Create(receipt.WithConcurrencyVersion(aggregate.Entity.ConcurrencyVersion));
    }

    // 事件处理器可能修改其他聚合并产生新事件，循环到没有新事件为止
    private async Task<Error?> DispatchDomainEventsAsync(ModuleDbContext context, CancellationToken ct)
    {
        var dispatcher = services.GetRequiredService<DomainEventDispatcher>();
        for (var round = 0; round < MaxDispatchRounds; round++)
        {
            var pending = new List<IDomainEvent>();
            foreach (var entry in context.ChangeTracker.Entries<IAggregateRoot>())
            {
                pending.AddRange(entry.Entity.DomainEvents);
                entry.Entity.ClearPendingRecords();
            }

            if (pending.Count == 0)
            {
                return null;
            }

            foreach (var domainEvent in pending)
            {
                var result = await dispatcher.DispatchAsync(domainEvent, ct);
                if (result.IsFailure)
                {
                    return result.Error;
                }
            }
        }

        throw new InvalidOperationException($"领域事件分发超过 {MaxDispatchRounds} 轮仍有新事件：事件处理器之间在互相触发。");
    }

    private static async Task RollBackAsync(ModuleDbContext context, CommandTransaction transaction)
    {
        // 清空变更跟踪器：同一作用域里残留的已修改实体不能被后续操作误保存
        await context.Database.RollbackTransactionAsync(CancellationToken.None);
        context.ChangeTracker.Clear();
        transaction.Discard();
    }

    private static async Task CommitAsync(IDbContextTransaction dbTransaction)
    {
        try
        {
            await dbTransaction.CommitAsync(CancellationToken.None);
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException or IOException)
        {
            throw new CommitOutcomeUnknownException("提交没有得到确认，结果未知。", exception);
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "提交后的动作限定为失败也无妨的事（05-02 D6），任何失败都只记警告，不能影响已经提交的结果")]
    private async Task RunAfterCommitAsync(IReadOnlyList<Func<CancellationToken, ValueTask>> actions)
    {
        foreach (var action in actions)
        {
            try
            {
                await action(CancellationToken.None);
            }
            catch (Exception exception)
            {
                PipelineLog.AfterCommitActionFailed(logger, exception, typeof(TMessage).Name);
            }
        }
    }
}
