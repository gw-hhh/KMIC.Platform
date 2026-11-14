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
/// Operations 模块的查询上下文：不跟踪实体，查询处理器通过 IOperationsQueryDb 使用
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

    // 执行记录表由框架的调度器写，不进模型；状态与触发方式在查询里就换成编码文字，取值见执行记录的状态约定
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

    // 路由表和心跳表由框架读写，不进模型；路由状态在查询里就换成编码文字，取值见路由状态约定
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
