using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Identity.Application.Maintenance;
using Npgsql;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 定时清理：已结束的会话、登录失败计数
/// </summary>
public sealed class IdentityMaintenanceTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private async Task<int> RunAsync<TCommand>(TCommand command)
        where TCommand : ICommand<Kmic.BuildingBlocks.Application.Scheduling.ScheduledJobOutcome>
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.ScheduledJob);
        var outcome = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(command, Ct);
        return outcome.ShouldBeSuccess().ItemsProcessed;
    }

    private async Task ExecuteAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using var data = NpgsqlDataSource.Create(ConnectionString);
        await using var command = data.CreateCommand(sql);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<Guid> SessionOfAsync(string account)
    {
        var token = (await LoginTokensAsync(account)).GetProperty("access_token").GetString()!;
        var sessions = await JsonAsync(await SendAsync(HttpMethod.Get, "/me/sessions", token));
        return sessions.EnumerateArray().Single(s => s.GetProperty("isCurrent").GetBoolean()).GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task PurgeSessions_RemovesSessionsThatEndedLongAgo_AndKeepsTheRest()
    {
        var role = await Seed.RoleAsync("清理-角色", []);
        await Seed.GrantAsync(await Seed.UserAsync("purge.user", "correct horse battery"), role);
        var expired = await SessionOfAsync("purge.user");
        var revokedLongAgo = await SessionOfAsync("purge.user");
        var revokedRecently = await SessionOfAsync("purge.user");
        var alive = await SessionOfAsync("purge.user");
        await ExecuteAsync("""UPDATE "Identity"."Session" SET "AbsoluteExpiresAt" = now() - interval '31 days' WHERE "Id" = @id""", ("id", expired));
        await ExecuteAsync("""UPDATE "Identity"."Session" SET "Status" = 'Revoked', "RevokedReason" = 'Logout', "RevokedAt" = now() - interval '31 days' WHERE "Id" = @id""", ("id", revokedLongAgo));
        await ExecuteAsync("""UPDATE "Identity"."Session" SET "Status" = 'Revoked', "RevokedReason" = 'Logout', "RevokedAt" = now() - interval '5 days' WHERE "Id" = @id""", ("id", revokedRecently));

        var purged = await RunAsync(new PurgeSessionsCommand(TimeProvider.System.GetUtcNow()));

        purged.ShouldBe(2);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."Session" WHERE "Id" IN ('{expired}', '{revokedLongAgo}')""")).ShouldBe(0);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."Session" WHERE "Id" IN ('{revokedRecently}', '{alive}')""")).ShouldBe(2);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."RefreshToken" WHERE "SessionId" IN ('{expired}', '{revokedLongAgo}')""")).ShouldBe(0);
    }

    [Fact]
    public async Task PurgeLoginThrottle_RemovesOldRows_ButKeepsRecentOnesAndThoseStillLocked()
    {
        const string Insert = """
            INSERT INTO "Identity"."LoginThrottle" ("LoginNameHash", "FailureCount", "LockLevel", "LockedUntil", "LastFailedAt")
            VALUES (@hash, 3, 1, @locked, @last)
            """;
        await ExecuteAsync(Insert, ("hash", "old"), ("locked", DBNull.Value), ("last", TimeProvider.System.GetUtcNow().AddDays(-3)));
        await ExecuteAsync(Insert, ("hash", "recent"), ("locked", DBNull.Value), ("last", TimeProvider.System.GetUtcNow().AddHours(-2)));
        await ExecuteAsync(Insert, ("hash", "locked"), ("locked", TimeProvider.System.GetUtcNow().AddHours(5)), ("last", TimeProvider.System.GetUtcNow().AddDays(-3)));

        var purged = await RunAsync(new PurgeLoginThrottleCommand(TimeProvider.System.GetUtcNow()));

        purged.ShouldBe(1);
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."LoginThrottle" WHERE "LoginNameHash" = 'old'""")).ShouldBe(0);
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."LoginThrottle" WHERE "LoginNameHash" IN ('recent', 'locked')""")).ShouldBe(2);
    }
}
