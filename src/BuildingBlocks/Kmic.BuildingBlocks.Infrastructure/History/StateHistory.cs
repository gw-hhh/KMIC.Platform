using System.Diagnostics;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.History;

/// <summary>
/// 业务历史：有状态机的聚合，每次状态流转写一行
/// </summary>
/// <remarks>
/// 流转在聚合里先记在内存，保存前写进历史表，和业务数据一起提交。不受 [Audited] 影响，只要流转了就写
/// </remarks>
internal static class StateHistory
{
    /// <summary>
    /// 标记只追加的表：建表迁移会收回运行账号的修改和删除权限
    /// </summary>
    public const string AppendOnlyAnnotation = "Kmic:AppendOnly";

    private const string TableSuffix = "StateHistory";
    private const BindingFlags StaticMembers = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    // 聚合根里有一个状态机的静态成员，就是有状态的聚合
    public static bool IsStateful(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.GetFields(StaticMembers).Any(f => IsMachine(f.FieldType)) || current.GetProperties(StaticMembers).Any(p => IsMachine(p.PropertyType)))
            {
                return true;
            }
        }

        return false;
    }

    public static string TableName(Type aggregate) => aggregate.Name + TableSuffix;

    /// <summary>
    /// 给模型里每个有状态的聚合映射一张历史表
    /// </summary>
    public static void Map(ModelBuilder modelBuilder)
    {
        var aggregates = modelBuilder.Model.GetEntityTypes()
            .Select(e => e.ClrType)
            .Where(t => EntityKinds.IsAggregateRoot(t) && IsStateful(t))
            .Distinct()
            .ToList();
        foreach (var aggregate in aggregates)
        {
            var name = TableName(aggregate);
            modelBuilder.SharedTypeEntity<StateHistoryRecord>(name, history =>
            {
                history.ToTable(name, table => table.HasComment($"{aggregate.Name} 的状态流转记录，只追加"));
                history.HasAnnotation(AppendOnlyAnnotation, true);
                history.HasKey(r => r.Id);
                history.Property(r => r.Id).ValueGeneratedNever();
                history.Property(r => r.StateName).HasMaxLength(100);
                history.Property(r => r.FromCode).HasMaxLength(100);
                history.Property(r => r.ToCode).HasMaxLength(100);
                history.Property(r => r.ActionCode).HasMaxLength(100);
                history.Property(r => r.Reason).HasMaxLength(2000);
                history.Property(r => r.TraceId).HasMaxLength(32);

                // 不建外键：聚合被物理删除之后，历史还要留着
                history.HasIndex(r => new { r.AggregateId, r.OccurredAt });
            });
        }
    }

    /// <summary>
    /// 把聚合里记着的流转写进历史表，写完清掉
    /// </summary>
    public static void Write(DbContext context, IExecutionContext execution, DateTimeOffset now)
    {
        var roots = context.ChangeTracker.Entries<IAggregateRoot>().Where(e => e.Entity.StateTransitions.Count > 0).ToList();
        if (roots.Count == 0)
        {
            return;
        }

        var user = execution.User;
        var traceId = Activity.Current?.TraceId.ToString() ?? new string('0', 32);
        foreach (var root in roots)
        {
            var name = TableName(root.Metadata.ClrType);
            if (context.Model.FindEntityType(name) is null)
            {
                throw new InvalidOperationException($"{root.Metadata.ClrType.Name} 有状态流转，但模型里没有 {name} 表。聚合根要用 private static readonly 字段声明状态机。");
            }

            var aggregateId = ((IStronglyTypedId)root.Property(nameof(Entity<>.Id)).CurrentValue!).Value;
            var history = context.Set<StateHistoryRecord>(name);
            foreach (var transition in root.Entity.StateTransitions)
            {
                history.Add(new StateHistoryRecord
                {
                    Id = Guid.CreateVersion7(),
                    AggregateId = aggregateId,
                    StateName = transition.StateName,
                    FromCode = transition.FromCode,
                    ToCode = transition.ToCode,
                    ActionCode = transition.ActionCode,
                    Reason = transition.Reason,
                    Revision = transition.Revision,
                    ActorUserId = user.UserId?.Value,
                    ActualUserId = user.ActualUserId?.Value,
                    RoleId = user.RoleId?.Value,
                    OnBehalfOfUserId = user.OnBehalfOf?.Value,
                    OccurredAt = now,
                    TraceId = traceId,
                });
            }

            root.Entity.ClearStateTransitions();
        }
    }

    private static bool IsMachine(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(StateMachine<,,>);
}
