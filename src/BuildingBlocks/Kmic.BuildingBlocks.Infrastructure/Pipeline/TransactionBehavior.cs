using System.Data;
using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>将命令执行、领域事件分发与持久化包裹在同一事务中</summary>
public sealed class TransactionBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    // 从服务容器解析框架内部服务：由于宿主需直接引用行为类型（必须为 public），构造函数无法直接注入 internal 服务
    IServiceProvider services,
    ILogger<TransactionBehavior<TMessage, TResponse>> logger) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseCommand
    where TResponse : Result, IResultFactory<TResponse>
{
    // 分发轮次超过上限仍有新事件产生，通常意味着事件处理器之间存在循环触发
    private const int MaxDispatchRounds = 10;

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var module = catalog.Get(typeof(TMessage)).Module;
        var context = services.GetRequiredService<ModuleWriteContexts>().Get(module);
        var transaction = services.GetRequiredService<CommandTransaction>();
        transaction.Module = module;
        await using var dbTransaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            return await ExecuteAsync(message, next, context, transaction, cancellationToken);
        }
        catch
        {
            // 发生异常时同步丢弃未保存修改、待执行动作及未发布消息，数据库事务由 await using 自动回滚
            context.ChangeTracker.Clear();
            transaction.Discard();
            services.GetRequiredService<OutboxWriter>().Discard();
            services.GetRequiredService<AuditRecorder>().Discard();
            throw;
        }
    }

    private async ValueTask<TResponse> ExecuteAsync(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        ModuleDbContext context,
        CommandTransaction transaction,
        CancellationToken ct)
    {
        var response = await next(message, ct);
        if (response.IsSuccess && await DispatchDomainEventsAsync(context, ct) is { } eventFailure)
        {
            response = TResponse.Fail(eventFailure);
        }

        if (response.IsFailure)
        {
            await RollBackAsync(context, transaction);
            return response;
        }

        if (catalog.Get(typeof(TMessage)) is { Audited: true } audited)
        {
            // 须在保存前采集变更：保存完成后变更跟踪器中的原始值将被清除
            services.GetRequiredService<AuditRecorder>().Publish(context, audited);
        }

        using (context.Services.SaveGate.Open())
        {
            await context.SaveChangesAsync(ct);
            var useCase = catalog.Get(typeof(TMessage));
            await services.GetRequiredService<OutboxWriter>().WriteAsync(context, useCase.Module, useCase.ExemptFromMessagingSuspension, ct);
        }

        response = WithSavedVersion(context, response);
        if (await transaction.RunBeforeCommitAsync(context, ResponseValues<TResponse>.Read(response), ct) is { } hookFailure)
        {
            await RollBackAsync(context, transaction);
            return TResponse.Fail(hookFailure);
        }

        await CommandCommit.CommitAsync(context.Database.CurrentTransaction ?? throw new InvalidOperationException("事务在提交前已结束，处理器不能自己提交或回滚。"));
        await RunAfterCommitAsync(transaction.TakeAfterCommit());
        return response;
    }

    // 并发版本号在保存阶段才会递增，因此需在保存完成后回填至响应结果
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

    // 事件处理器可能修改其他聚合根并产生衍生领域事件，需循环分发直至无新增事件
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

        throw new InvalidOperationException($"领域事件分发超过 {MaxDispatchRounds} 轮仍有新事件，事件处理器可能在互相触发。");
    }

    private async Task RollBackAsync(ModuleDbContext context, CommandTransaction transaction)
    {
        // 清空变更跟踪器，防止异常路径上的残留状态被后续操作误保存
        await context.Database.RollbackTransactionAsync(CancellationToken.None);
        context.ChangeTracker.Clear();
        transaction.Discard();
        services.GetRequiredService<OutboxWriter>().Discard();
        services.GetRequiredService<AuditRecorder>().Discard();
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "提交后动作失败仅记录警告日志，不能影响已成功提交的命令结果")]
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
