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

/// <summary>EF Core 仓储基类</summary>
public abstract class EfRepository<TContext, TAggregate, TId>(TContext context) : IRepository<TAggregate, TId>
    where TContext : ModuleDbContext
    where TAggregate : AggregateRoot<TId>
    where TId : struct, IStronglyTypedId
{
    protected TContext Context => context;

    /// <summary>未找到聚合时返回的错误（如 SampleErrors.NotFound）</summary>
    protected abstract Error NotFound { get; }

    private DbSet<TAggregate> Set => context.Set<TAggregate>();

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

    public async Task<Result<TAggregate>> GetForUpdateLockedAsync(TId id, long? expectedVersion, CancellationToken ct)
    {
        await LockAsync(id, ct);
        return await GetForUpdateAsync(id, expectedVersion, ct);
    }

    public void Add(TAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        Set.Add(aggregate);
    }

    public void Remove(TAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        Set.Remove(aggregate);
    }

    /// <summary>恢复已软删除的聚合（模块仓储接口声明同名方法即可暴露；聚合未实现 ISoftDeletable 时抛异常）</summary>
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

    /// <summary>配置读取时包含的子实体导航，默认只读聚合根</summary>
    protected virtual IQueryable<TAggregate> Aggregate(IQueryable<TAggregate> set) => set;

    /// <summary>按条件直接批量更新（须在事务内调用；同命令内勿随后再经仓储读写同一记录，以免跟踪实体持有旧值）</summary>
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

        // 此处绕过查询过滤器以直接锁定目标行，数据可见性由随后的实体读取负责校验
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
