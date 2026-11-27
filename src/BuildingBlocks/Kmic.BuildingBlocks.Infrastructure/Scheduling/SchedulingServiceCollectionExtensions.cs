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

public static class SchedulingServiceCollectionExtensions
{
    /// <summary>注册调度器客户端、定时任务目录与启动同步任务（Api 与 Worker 均需调用；Worker 须额外调用 AddKmicSchedulerWorker）</summary>
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
        // 调度器直接复用 DI 容器中的 TimeProvider；此处不显式配置，避免 Quartz 将其注册为全局时钟并覆盖宿主时钟
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

    /// <summary>启用本实例的定时任务调度与执行能力（供 Worker 角色调用）</summary>
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
