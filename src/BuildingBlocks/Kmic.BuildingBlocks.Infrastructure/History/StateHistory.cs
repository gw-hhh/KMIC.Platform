using System.Diagnostics;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.History;

// 状态流转历史记录器（每次状态机流转后写入只追加记录）
internal static class StateHistory
{
    // 建表迁移据此回收运行账号的 UPDATE/DELETE 权限
    public const string AppendOnlyAnnotation = "Kmic:AppendOnly";

    private const string TableSuffix = "StateHistory";
    private const BindingFlags StaticMembers = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    // 凡包含状态机静态成员的聚合根均视为有状态聚合
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

                // 不建立外键约束，确保聚合根物理删除后历史记录仍保留
                history.HasIndex(r => new { r.AggregateId, r.OccurredAt });
            });
        }
    }

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
                throw new InvalidOperationException($"{root.Metadata.ClrType.Name} 存在状态流转，但模型中未配置 {name} 表。聚合根须通过 private static readonly 字段声明状态机。");
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
