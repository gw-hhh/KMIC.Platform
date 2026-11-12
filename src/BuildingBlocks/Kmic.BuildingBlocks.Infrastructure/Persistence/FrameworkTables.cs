using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Numbering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 框架表
/// </summary>
/// <remarks>
/// 每个模块的 schema 里都有，和业务数据在同一个事务里写入
/// </remarks>
internal static class FrameworkTables
{
    public const string Idempotency = nameof(IdempotencyRecord);

    public const string Outbox = nameof(OutboxMessage);

    public const string Inbox = nameof(InboxMessage);

    public const string Jobs = "BackgroundJob";

    public const string RebuildRuns = nameof(ReadModelRebuildRun);

    public const string NumberCounters = "NumberCounter";

    // 只索引还没完成的消息：接收方删除对象前只关心这些
    private const string UnfinishedWithSubjects = """
        "SubjectIds" IS NOT NULL AND "Status" IN (0, 1, 3)
        """;

    private const string Claimable = """
        "Status" IN (0, 1)
        """;

    // 还没处理完的和死信：积压统计按订阅分组，只看这些行
    private const string Outstanding = """
        "Status" IN (0, 1, 3)
        """;

    private const string OrderedUnfinished = """
        "Status" IN (0, 1, 3) AND "OrderingKey" IS NOT NULL
        """;

    // 清理时只找已完成的：Inbox 的去重凭证、到期的作业记录
    private const string Done = """
        "Status" = 2
        """;

    private const string Finished = """
        "ExpiresAt" IS NOT NULL
        """;

    // 同一个订阅同时只能有一轮没结束的重建
    private const string ActiveRun = """
        "Status" IN (0, 1)
        """;

    private const string Unfinished = """
        "Status" IN (0, 1, 4)
        """;

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdempotencyRecord>(record =>
        {
            record.ToTable(Idempotency, t => t.HasComment("幂等记录"));
            record.Property(r => r.Id).ValueGeneratedNever();
            record.Property(r => r.Operation).HasMaxLength(200);
            record.Property(r => r.Key).HasMaxLength(100);
            record.Property(r => r.Status).HasMaxLength(32);
            record.Property(r => r.ResponseBody).HasColumnType(ColumnTypes.Json);
            record.HasIndex(r => new { r.UserId, r.RoleId, r.Operation, r.Key }).IsUnique();
            record.HasIndex(r => r.ExpiresAt);
        });

        modelBuilder.Entity<OutboxMessage>(message =>
        {
            message.ToTable(Outbox, t => t.HasComment("待投递的集成消息"));
            message.Property(m => m.Id).ValueGeneratedNever();
            message.Property(m => m.Seq).UseIdentityAlwaysColumn();
            MapCommonColumns(message);
            message.HasIndex(m => m.NextAttemptAt).HasFilter(Claimable);
            message.HasIndex(m => new { m.Status, m.OccurredAt });
            message.HasIndex(m => m.SubjectIds).HasMethod("gin").HasFilter(UnfinishedWithSubjects);
        });

        modelBuilder.Entity<InboxMessage>(message =>
        {
            message.ToTable(Inbox, t => t.HasComment("待处理的集成消息，每个订阅一行"));
            message.Property(m => m.Id).ValueGeneratedNever();
            message.Property(m => m.Subscription).HasMaxLength(200);
            message.Property(m => m.SourceModule).HasMaxLength(100);
            message.Property(m => m.ReceivedAt).HasDefaultValueSql("clock_timestamp()");
            MapCommonColumns(message);
            message.HasIndex(m => new { m.MessageId, m.Subscription }).IsUnique();
            message.HasIndex(m => m.NextAttemptAt).HasFilter(Claimable);
            message.HasIndex(m => new { m.Subscription, m.OrderingKey, m.Seq }).HasFilter(OrderedUnfinished);
            message.HasIndex(m => m.ReceivedAt).HasFilter(Done);
            message.HasIndex(m => m.Subscription).HasFilter(Outstanding);
            message.HasIndex(m => m.SubjectIds).HasMethod("gin").HasFilter(UnfinishedWithSubjects);
        });

        modelBuilder.Entity<BackgroundJobRecord>(MapJobs);
        modelBuilder.Entity<NumberCounterRecord>(MapNumberCounters);
        modelBuilder.Entity<ReadModelRebuildRun>(MapRebuildRuns);
    }

    private static void MapNumberCounters(EntityTypeBuilder<NumberCounterRecord> counter)
    {
        counter.ToTable(NumberCounters, t => t.HasComment("业务编号的计数"));
        counter.HasKey(c => new { c.Rule, c.PeriodKey, c.PartitionKey });
        counter.Property(c => c.Rule).HasMaxLength(100);
        counter.Property(c => c.PeriodKey).HasMaxLength(8);
        counter.Property(c => c.PartitionKey).HasMaxLength(10);
    }

    private static void MapRebuildRuns(EntityTypeBuilder<ReadModelRebuildRun> run)
    {
        run.ToTable(RebuildRuns, t => t.HasComment("读模型的重建轮次"));
        run.Property(r => r.Id).ValueGeneratedNever();
        run.Property(r => r.Subscription).HasMaxLength(200);
        run.Property(r => r.ReadModel).HasMaxLength(200);
        run.Property(r => r.StartedAt).HasDefaultValueSql("clock_timestamp()");
        run.HasIndex(r => r.Subscription).IsUnique().HasFilter(ActiveRun);
    }

    private static void MapJobs(EntityTypeBuilder<BackgroundJobRecord> job)
    {
        job.ToTable(Jobs, t => t.HasComment("后台作业"));
        job.Property(j => j.Id).ValueGeneratedNever();
        job.Property(j => j.JobType).HasMaxLength(200);
        job.Property(j => j.Args).HasColumnType(ColumnTypes.Json);
        job.Property(j => j.Checkpoint).HasColumnType(ColumnTypes.Json);
        job.Property(j => j.ResultSummary).HasColumnType(ColumnTypes.Json);
        job.Property(j => j.ProgressMessage).HasMaxLength(500);
        job.Property(j => j.ErrorCode).HasMaxLength(200);
        job.Property(j => j.ErrorMessage).HasMaxLength(2000);
        job.HasIndex(j => j.LeaseUntil).HasFilter(Unfinished);
        job.HasIndex(j => j.ExpiresAt).HasFilter(Finished);
        job.HasIndex(j => new { j.SubmittedBy, j.SubmittedAt }).IsDescending(false, true);
    }

    // 两张表共有的列。时间默认取数据库时钟，各实例的时钟有偏差也不影响
    private static void MapCommonColumns<T>(EntityTypeBuilder<T> message)
        where T : class
    {
        message.Property<string>(nameof(OutboxMessage.MessageType)).HasMaxLength(200);
        message.Property<string>(nameof(OutboxMessage.Payload)).HasColumnType(ColumnTypes.Json);
        message.Property<string>(nameof(OutboxMessage.Headers)).HasColumnType(ColumnTypes.Json);
        message.Property<string?>(nameof(OutboxMessage.OrderingKey)).HasMaxLength(100);
        message.Property<string?>(nameof(OutboxMessage.LastError)).HasMaxLength(4000);
        message.Property<DateTimeOffset>(nameof(OutboxMessage.NextAttemptAt)).HasDefaultValueSql("clock_timestamp()");
    }
}
