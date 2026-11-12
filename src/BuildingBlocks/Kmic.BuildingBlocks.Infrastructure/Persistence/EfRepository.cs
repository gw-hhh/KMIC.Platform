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
/// 仓储基类
/// </summary>
/// <remarks>
/// 不存在、已删除或没有权限时都返回 NotFound；没有权限的情况另外记安全事件
/// </remarks>
/// <param name="context">写入上下文</param>
public abstract class EfRepository<TContext, TAggregate, TId>(TContext context) : IRepository<TAggregate, TId>
    where TContext : ModuleDbContext
    where TAggregate : AggregateRoot<TId>
    where TId : struct, IStronglyTypedId
{
    /// <summary>
    /// 写入上下文
    /// </summary>
    protected TContext Context => context;

    /// <summary>
    /// 找不到时返回的错误，如 SampleErrors.NotFound
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
    /// <exception cref="InvalidOperationException">不在事务里</exception>
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
    /// 恢复已删除的数据
    /// </summary>
    /// <remarks>
    /// 数据权限照常生效。模块仓储接口要恢复功能时，声明同名方法即可
    /// </remarks>
    /// <exception cref="InvalidOperationException">不支持软删除</exception>
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
    /// 读取时要包含的子实体，默认只读聚合根
    /// </summary>
    protected virtual IQueryable<TAggregate> Aggregate(IQueryable<TAggregate> set) => set;

    /// <summary>
    /// 按条件直接更新
    /// </summary>
    /// <remarks>
    /// 用于计数、扣减额度这类单表操作，返回更新的行数。不经过变更跟踪，修改人、修改时间和版本号照常更新，过滤器照常生效。
    /// 同一个命令里不要再通过仓储读取并修改同一条数据，否则跟踪的实体是旧值
    /// </remarks>
    /// <param name="condition">更新条件，如 q => q.Id == id &amp;&amp; q.Remaining &gt;= amount</param>
    /// <param name="setters">要更新的列，如 s => s.SetProperty(q => q.Remaining, q => q.Remaining - amount)</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="InvalidOperationException">不在事务里</exception>
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

    private static Expression<Func<TAggregate, bool>> HasId(TId id) => EntityPredicates.HasId<TAggregate, TId>(id);

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

        // 这里不经过过滤器：锁的只是这一行，能不能看到在接下来的读取里判断
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
            throw new InvalidOperationException($"{operation} 必须在事务里调用。");
        }
    }
}
