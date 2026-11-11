using System.Linq.Expressions;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 仓储基类：按 ID 加载完整聚合，登记新增与删除；模块仓储只需给出 Include 链与“找不到”的错误
/// </summary>
/// <remarks>
/// 加载不到（不存在、已删除、在数据范围之外）一律返回 NotFound；范围之外的真实原因写入安全事件，不告诉调用方
/// </remarks>
/// <param name="context">模块的写入上下文</param>
public abstract class EfRepository<TContext, TAggregate, TId>(TContext context) : IRepository<TAggregate, TId>
    where TContext : ModuleDbContext
    where TAggregate : AggregateRoot<TId>
    where TId : struct, IStronglyTypedId
{
    /// <summary>
    /// 模块的写入上下文，供模块仓储写按业务键加载等查询
    /// </summary>
    protected TContext Context => context;

    /// <summary>
    /// 聚合加载不到时返回的模块错误，如 SampleErrors.NotFound
    /// </summary>
    protected abstract Error NotFound { get; }

    private DbSet<TAggregate> Set => context.Set<TAggregate>();

    /// <inheritdoc />
    public async Task<Result<TAggregate>> GetForUpdateAsync(TId id, long? expectedVersion, CancellationToken ct)
    {
        var aggregate = await Aggregate(Set).AsSplitQuery().SingleOrDefaultAsync(HasId(id), ct);
        if (aggregate is null)
        {
            await RecordIfOutOfScopeAsync(id, ct);
            return NotFound;
        }

        return CheckVersion(aggregate, expectedVersion);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">不在事务中：行锁会随语句结束立即释放，起不到作用</exception>
    public async Task<Result<TAggregate>> GetForUpdateLockedAsync(TId id, long? expectedVersion, CancellationToken ct)
    {
        await LockAsync(id, ct);
        return await GetForUpdateAsync(id, expectedVersion, ct);
    }

    /// <inheritdoc />
    public void Add(TAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        Set.Add(aggregate);
    }

    /// <inheritdoc />
    public void Remove(TAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        Set.Remove(aggregate);
    }

    /// <summary>
    /// 恢复已软删除的聚合，随事务保存；数据范围照常生效。模块仓储接口需要恢复时声明同名方法即可
    /// </summary>
    /// <exception cref="InvalidOperationException">聚合不支持软删除</exception>
    public async Task<Result<TAggregate>> RestoreAsync(TId id, long? expectedVersion, CancellationToken ct)
    {
        if (!typeof(ISoftDeletable).IsAssignableFrom(typeof(TAggregate)))
        {
            throw new InvalidOperationException($"{typeof(TAggregate).Name} 没有实现 ISoftDeletable，不能恢复。");
        }

        var aggregate = await Aggregate(Set).IgnoreQueryFilters([QueryFilters.SoftDelete]).AsSplitQuery().SingleOrDefaultAsync(HasId(id), ct);
        if (aggregate is null)
        {
            await RecordIfOutOfScopeAsync(id, ct);
            return NotFound;
        }

        var checkedVersion = CheckVersion(aggregate, expectedVersion);
        if (checkedVersion.IsSuccess)
        {
            var entry = context.Entry(aggregate);
            entry.Property(SoftDeleteColumns.IsDeleted).CurrentValue = false;
            entry.Property(SoftDeleteColumns.DeletedAt).CurrentValue = null;
            entry.Property(SoftDeleteColumns.DeletedBy).CurrentValue = null;
        }

        return checkedVersion;
    }

    /// <summary>
    /// 一次性加载完整聚合要用的 Include 链，默认只有聚合根
    /// </summary>
    protected virtual IQueryable<TAggregate> Aggregate(IQueryable<TAggregate> set) => set;

    /// <summary>
    /// 原子条件更新：条件成立的行才更新，返回影响行数；为 0 时由调用方返回业务错误。用于计数、额度扣减等单表操作
    /// </summary>
    /// <remarks>
    /// 直接执行 SQL，不经过变更跟踪：框架同时写入修改人、修改时间并把版本加一，软删除与数据范围过滤器照常生效。
    /// 同一个命令中不要再经仓储加载并修改同一个聚合，否则被跟踪的实体是旧值
    /// </remarks>
    /// <param name="condition">更新条件，如 q => q.Id == id &amp;&amp; q.Remaining &gt;= amount</param>
    /// <param name="setters">要更新的列，如 s => s.SetProperty(q => q.Remaining, q => q.Remaining - amount)</param>
    /// <param name="ct">取消</param>
    /// <exception cref="InvalidOperationException">不在事务中</exception>
    protected async Task<int> UpdateWhereAsync(
        Expression<Func<TAggregate, bool>> condition,
        Action<UpdateSettersBuilder<TAggregate>> setters,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(setters);
        EnsureInTransaction(nameof(UpdateWhereAsync));

        var now = context.Services.Time.GetUtcNow();
        var actor = SaveRules.Actor(context.Services.Execution.User);
        return await Set.Where(condition).ExecuteUpdateAsync(
            update =>
            {
                setters(update);
                update
                    .SetProperty(a => EF.Property<DateTimeOffset?>(a, AuditColumns.UpdatedAt), now)
                    .SetProperty(a => EF.Property<UserId?>(a, AuditColumns.UpdatedBy), actor)
                    .SetProperty(a => EF.Property<long>(a, nameof(IAggregateRoot.ConcurrencyVersion)), a => EF.Property<long>(a, nameof(IAggregateRoot.ConcurrencyVersion)) + 1);
            },
            ct);
    }

    // 参数放在闭包对象上，EF 把它当作 SQL 参数，不同 ID 共用一份编译结果
    private static Expression<Func<TAggregate, bool>> HasId(TId id)
    {
        var aggregate = Expression.Parameter(typeof(TAggregate), "a");
        var value = Expression.Property(Expression.Constant(new IdHolder(id)), nameof(IdHolder.Value));
        return Expression.Lambda<Func<TAggregate, bool>>(Expression.Equal(Expression.Property(aggregate, nameof(Entity<>.Id)), value), aggregate);
    }

    private static Result<TAggregate> CheckVersion(TAggregate aggregate, long? expectedVersion) =>
        expectedVersion is { } expected && expected != aggregate.ConcurrencyVersion
            ? CommonErrors.VersionConflict.WithMetadata(ErrorMetadataKeys.CurrentVersion, aggregate.ConcurrencyVersion)
            : aggregate;

    private async Task RecordIfOutOfScopeAsync(TId id, CancellationToken ct)
    {
        var entityType = context.Model.FindEntityType(typeof(TAggregate));
        if (entityType?.GetDeclaredQueryFilters().Any(f => string.Equals(f.Key, QueryFilters.DataScope, StringComparison.Ordinal)) != true
            || !await Set.IgnoreQueryFilters([QueryFilters.DataScope]).AnyAsync(HasId(id), ct))
        {
            return;
        }

        context.Services.SecurityEvents.Write(new SecurityEvent("AuthorizationDenied", SecuritySeverity.Warning, nameof(DenialReason.OutOfDataScope))
        {
            UserId = context.Services.Execution.User.UserId?.Value,
            ResourceType = typeof(TAggregate).Name,
            ResourceId = id.Value,
        });
    }

    private async Task LockAsync(TId id, CancellationToken ct)
    {
        EnsureInTransaction(nameof(GetForUpdateLockedAsync));

        // 标识符取自模型、带引号，ID 是参数；不经过过滤器：锁的是行本身，可见性在随后的加载中判断
        var entityType = context.Model.FindEntityType(typeof(TAggregate))
            ?? throw new InvalidOperationException($"{typeof(TAggregate).Name} 不在 {context.Module} 模块的模型中。");
        var idColumn = entityType.FindProperty(nameof(Entity<>.Id))!.GetColumnName();
        var sql = $"""SELECT 1 FROM "{entityType.GetSchema()}"."{entityType.GetTableName()}" WHERE "{idColumn}" = @id FOR UPDATE""";
        await context.Database.ExecuteSqlRawAsync(sql, [new NpgsqlParameter("id", id.Value)], ct);
    }

    private void EnsureInTransaction(string operation)
    {
        if (context.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException($"{operation} 只能在事务中调用：命令由事务行为开启事务后才执行处理器。");
        }
    }

    private sealed record IdHolder(TId Value);
}
