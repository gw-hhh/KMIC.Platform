using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Operations.Application;
using Kmic.Operations.Application.Messages.Instances;
using Kmic.Operations.Application.Messages.Routes;
using Kmic.Operations.Application.ScheduledJobs;
using Kmic.Operations.Contracts;
using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Infrastructure.Persistence;

/// <summary>
/// Operations 模块的查询上下文，不跟踪实体
/// </summary>
internal sealed class OperationsQueryDbContext(DbContextOptions<OperationsQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IOperationsQueryDb
{
    /// <inheritdoc />
    public override string Module => OperationsModuleInfo.Name;

    /// <inheritdoc />
    public IQueryable<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    public IQueryable<ScheduledJobSetting> ScheduledJobSettings => Set<ScheduledJobSetting>();

    // 执行记录由框架调度器维护，不纳入模块 EF 模型
    /// <inheritdoc />
    public IQueryable<ScheduledJobRunRow> ScheduledJobRuns => Database.SqlQuery<ScheduledJobRunRow>(
        $"""
        SELECT "Id", "JobName", "ScheduledFireTime", "StartedAt", "FinishedAt", "DurationMs",
               CASE "Trigger" WHEN 1 THEN 'Manual' WHEN 2 THEN 'Misfire' ELSE 'Schedule' END AS "Trigger", "TriggeredBy",
               CASE "Status" WHEN 0 THEN 'Running' WHEN 1 THEN 'Succeeded' WHEN 2 THEN 'Failed' WHEN 3 THEN 'Unknown' ELSE 'Abandoned' END AS "Status",
               "ItemsProcessed", "Note", "ErrorCode", "ErrorMessage", "InstanceId", "TraceId"
        FROM "Operations"."ScheduledJobRun"
        """);

    /// <inheritdoc />
    public IQueryable<ScheduledJobRunRow> LatestScheduledJobRuns => Database.SqlQuery<ScheduledJobRunRow>(
        $"""
        SELECT DISTINCT ON ("JobName") "Id", "JobName", "ScheduledFireTime", "StartedAt", "FinishedAt", "DurationMs",
               CASE "Trigger" WHEN 1 THEN 'Manual' WHEN 2 THEN 'Misfire' ELSE 'Schedule' END AS "Trigger", "TriggeredBy",
               CASE "Status" WHEN 0 THEN 'Running' WHEN 1 THEN 'Succeeded' WHEN 2 THEN 'Failed' WHEN 3 THEN 'Unknown' ELSE 'Abandoned' END AS "Status",
               "ItemsProcessed", "Note", "ErrorCode", "ErrorMessage", "InstanceId", "TraceId"
        FROM "Operations"."ScheduledJobRun"
        ORDER BY "JobName", "StartedAt" DESC, "Id" DESC
        """);

    // 路由和心跳由框架维护，不纳入模块 EF 模型
    /// <inheritdoc />
    public IQueryable<MessageRouteRow> MessageRoutes => Database.SqlQuery<MessageRouteRow>(
        $"""
        SELECT "MessageType", "Subscription", "Module",
               CASE "Status" WHEN 0 THEN 'Active' WHEN 1 THEN 'Retiring' ELSE 'Finalized' END AS "Status",
               "RegisteredBuild", "RegisteredAt", "NeedsInitialization", "DisplayName", "RetiredReason", "FinalizedAt", "FinalizedBy"
        FROM "Operations"."MessageRoute"
        """);

    /// <inheritdoc />
    public IQueryable<MessagingInstanceRow> MessagingInstances => Database.SqlQuery<MessagingInstanceRow>(
        $"""
        SELECT "Id", "Host", "Roles", "Build", "Subscriptions"::text AS "Subscriptions", "StartedAt", "HeartbeatAt"
        FROM "Operations"."MessagingInstance"
        """);

    /// <inheritdoc />
    public IQueryable<AuditLog> WithTarget(IQueryable<AuditLog> audits, string targetsJson)
    {
        ArgumentNullException.ThrowIfNull(audits);

        return audits.Where(a => EF.Functions.JsonContains(a.Targets, targetsJson));
    }

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInOperationsIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => OperationsModel.Configure(modelBuilder);
}
