using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Maintenance;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 定时任务注册
/// </summary>
public static class SchedulingServiceCollectionExtensions
{
    /// <summary>
    /// 注册调度器（待机）、定时任务目录、框架的清理任务和启动时的同步，Api、Worker 两种角色都要调用
    /// </summary>
    /// <remarks>
    /// 调度器使用 Operations 里的 Quartz 表，集群模式，多个实例同一次触发只执行一次。
    /// 只调用这个方法的实例不执行任务，只能手动执行和改时间；要执行再调用 <see cref="AddKmicSchedulerWorker"/>
    /// </remarks>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddKmicScheduler(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(sp => ScheduledJobCatalog.Build(
            sp.GetServices<ModuleSchedules>(), sp.GetRequiredService<UseCaseCatalog>(), CheckCron));
        services.AddOptions<PurgeOptions>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFrameworkTask, PurgeIdempotencyTask>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFrameworkTask, PurgeMessagesTask>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFrameworkTask, PurgeJobsTask>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFrameworkTask, PurgeScheduledJobRunsTask>());
        services.TryAddSingleton<ScheduledTasks>();
        services.TryAddSingleton<ScheduledJobRuns>();
        services.TryAddSingleton<ScheduledJobSettings>();
        services.TryAddSingleton<ScheduledCommandRunner>();
        services.TryAddSingleton<ScheduleSynchronizer>();
        services.TryAddSingleton<IScheduledJobControl, ScheduledJobControl>();
        services.AddTransient<CommandJob>();
        services.AddScoped<IStartupTask, SyncSchedulesTask>();
        // 调度器用容器里的 TimeProvider。不在这里指定：Quartz 会把它注册成全容器的时钟，盖掉宿主的
        services.AddQuartz(q =>
        {
            q.UsePersistentStore(store =>
            {
                store.UsePostgres(source => source.DataSourceFactory = sp => sp.GetRequiredService<DataSources>().Transactions(CallOrigin.ScheduledJob));
                store.ConfigureStore(options =>
                {
                    options.TablePrefix = SchedulingTables.QuartzPrefix;
                    options.StoreJobDataAsStrings = true;
                    options.MisfireThreshold = CommandJob.MisfireThreshold;
                });
                store.UseClustering();
                store.UseSystemTextJsonSerializer();
            });
        });
        return services;
    }

    /// <summary>
    /// 在本实例上执行定时任务，Worker 角色调用
    /// </summary>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddKmicSchedulerWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<StartupState>();
        services.AddHostedService<SchedulerService>();
        return services;
    }

    private static string? CheckCron(string cron)
    {
        try
        {
            _ = CronExpression.Parse(cron);
            return null;
        }
        catch (FormatException exception)
        {
            return exception.Message;
        }
    }
}
