using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 定期登记本实例的心跳：构建号、角色、声明的订阅
/// </summary>
/// <remarks>
/// Api 和 Worker 都登记，都会声明订阅。数据库暂时连不上时记一条警告，下一个间隔再试；
/// 等启动任务做完才开始，Operations 的表才一定存在
/// </remarks>
internal sealed class InstanceHeartbeat(
    SubscriptionCatalog subscriptions,
    DataSources dataSources,
    StartupState startup,
    InstanceIdentity identity,
    TimeProvider time,
    IOptions<MessagingOptions> options,
    ILogger<InstanceHeartbeat> logger) : BackgroundService
{
    // 清单里的属性名用小写开头：终结订阅时按包含运算匹配，名字的大小写要固定
    private static readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await startup.WaitAsync(stoppingToken);
            var startedAt = time.GetUtcNow();
            using var timer = new PeriodicTimer(options.Value.HeartbeatInterval, time);
            do
            {
                await TryBeatAsync(startedAt, stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    /// <summary>
    /// 登记一次心跳：第一次插入，之后更新心跳时间；顺手删掉早就没有心跳的旧行
    /// </summary>
    public async Task BeatAsync(DateTimeOffset startedAt, CancellationToken ct)
    {
        var declared = subscriptions.All
            .Select(s => new { MessageType = s.Message.Name, Subscription = s.Name, s.Module })
            .Distinct()
            .OrderBy(s => s.Module, StringComparer.Ordinal)
            .ThenBy(s => s.Subscription, StringComparer.Ordinal)
            .ToList();
        await using var command = dataSources.Transactions(CallOrigin.Inbox).CreateCommand(
            $"""
            INSERT INTO {MessagingInstanceTable.Name} ("Id", "Host", "Roles", "Build", "Subscriptions", "StartedAt", "HeartbeatAt")
            VALUES (@id, @host, @roles, @build, @subscriptions, @started, clock_timestamp())
            ON CONFLICT ("Id") DO UPDATE SET "HeartbeatAt" = clock_timestamp(), "Subscriptions" = EXCLUDED."Subscriptions";
            DELETE FROM {MessagingInstanceTable.Name} WHERE "HeartbeatAt" < clock_timestamp() - @retention
            """);
        command.Parameters.AddWithValue("id", identity.Id);
        command.Parameters.AddWithValue("host", identity.Host);
        command.Parameters.AddWithValue("roles", identity.Roles);
        command.Parameters.AddWithValue("build", BuildInfo.Version);
        command.Parameters.Add(new NpgsqlParameter("subscriptions", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = JsonSerializer.Serialize(declared, _json) });
        command.Parameters.AddWithValue("started", startedAt);
        command.Parameters.AddWithValue("retention", MessagingInstanceTable.Retention);
        await command.ExecuteNonQueryAsync(ct);
    }

    private async Task TryBeatAsync(DateTimeOffset startedAt, CancellationToken ct)
    {
        try
        {
            await BeatAsync(startedAt, ct);
        }
        catch (Exception exception) when (exception is NpgsqlException or InvalidOperationException)
        {
            MessagingLog.HeartbeatFailed(logger, exception);
        }
    }
}
