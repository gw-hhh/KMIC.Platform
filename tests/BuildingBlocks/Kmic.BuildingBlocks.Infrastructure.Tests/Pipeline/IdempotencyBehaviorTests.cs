using System.Security.Cryptography;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Pipeline;

public sealed class IdempotencyBehaviorTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const string Operation = "BuildingBlocks.OpenTicketCommand";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task MissingKey_Rejected()
    {
        (await SendAsync(new OpenTicketCommand(TicketNumbers.Next(), "无键"), key: null)).Result.ShouldBeFailure(CommonErrors.IdempotencyKeyRequired);
        _harness.Hooks.Handled.ShouldBe(0);
    }

    [Fact]
    public async Task SameKeySameRequest_ExecutesOnceAndReplaysFirstResult()
    {
        var key = NewKey();
        var command = new OpenTicketCommand(TicketNumbers.Next(), "首次");

        var first = await SendAsync(command, key);
        var second = await SendAsync(command, key);

        second.Result.ShouldBeSuccess().ShouldBe(first.Result.ShouldBeSuccess());
        first.Replayed.ShouldBeFalse();
        second.Replayed.ShouldBeTrue();
        _harness.Hooks.Handled.ShouldBe(1);
    }

    [Fact]
    public async Task SameKeyDifferentRequest_Rejected()
    {
        var key = NewKey();
        (await SendAsync(new OpenTicketCommand(TicketNumbers.Next(), "甲"), key)).Result.ShouldBeSuccess();

        (await SendAsync(new OpenTicketCommand(TicketNumbers.Next(), "乙"), key)).Result.ShouldBeFailure(CommonErrors.IdempotencyKeyReused);
    }

    [Fact]
    public async Task HandlerFailure_ReleasesKey_RetryWithSameKeyRuns()
    {
        var key = NewKey();
        var number = TicketNumbers.Next();
        (await SendAsync(new OpenTicketCommand(number, OpenTicketCommand.RejectedTitle), key)).Result.ShouldBeFailure(TicketErrors.Rejected);

        // 失败后记录已删除，同一个键可以再用
        (await SendAsync(new OpenTicketCommand(number, "修正后"), key)).Result.ShouldBeSuccess();
        _harness.Hooks.Handled.ShouldBe(2);
    }

    [Fact]
    public async Task ProcessingWithValidLease_InProgress()
    {
        var key = NewKey();
        var command = new OpenTicketCommand(TicketNumbers.Next(), "处理中");
        await InsertProcessingAsync(command, key, leaseExpiresAt: _harness.Time.GetUtcNow().AddMinutes(1));

        var result = (await SendAsync(command, key)).Result;

        result.ShouldBeFailure(CommonErrors.RequestInProgress).Metadata[ErrorMetadataKeys.Retryable].ShouldBe(true);
        _harness.Hooks.Handled.ShouldBe(0);
    }

    [Fact]
    public async Task ProcessingWithExpiredLease_TakenOver()
    {
        // 原来的请求在提交前崩溃了，只留下一条处理中的记录
        var key = NewKey();
        var command = new OpenTicketCommand(TicketNumbers.Next(), "接管");
        await InsertProcessingAsync(command, key, leaseExpiresAt: _harness.Time.GetUtcNow().AddSeconds(-1));

        (await SendAsync(command, key)).Result.ShouldBeSuccess();
        (await SendAsync(command, key)).Replayed.ShouldBeTrue();
        _harness.Hooks.Handled.ShouldBe(1);
    }

    [Fact]
    public async Task TakenOverWhileRunning_OriginalCannotCommit()
    {
        var key = NewKey();
        var number = TicketNumbers.Next();
        _harness.Hooks.OnBeforeReturn(async ct =>
        {
            // 模拟处理器还在运行时，记录被另一个请求接管
            await using var command = database.DataSource.CreateCommand(
                """UPDATE "Probe"."IdempotencyRecord" SET "LeaseToken" = $1 WHERE "Key" = $2""");
            command.Parameters.Add(new NpgsqlParameter { Value = Guid.CreateVersion7() });
            command.Parameters.Add(new NpgsqlParameter { Value = key });
            await command.ExecuteNonQueryAsync(ct);
        });

        var result = (await SendAsync(new OpenTicketCommand(number, "被接管"), key)).Result;

        result.ShouldBeFailure(CommonErrors.RequestInProgress);
        await using var check = _harness.BeginAsSystem();
        check.Query.Tickets.Any(t => t.Number == number).ShouldBeFalse();
    }

    [Fact]
    public async Task ConcurrentSameKey_OnlyOneExecutes()
    {
        var key = NewKey();
        var command = new OpenTicketCommand(TicketNumbers.Next(), "并发");
        var firstInsideHandler = new TaskCompletionSource();
        var releaseFirst = new TaskCompletionSource();
        _harness.Hooks.OnBeforeReturn(async _ =>
        {
            firstInsideHandler.TrySetResult();
            await releaseFirst.Task;
        });

        var first = SendAsync(command, key);
        await firstInsideHandler.Task.WaitAsync(Ct);
        var second = await SendAsync(command, key);
        releaseFirst.SetResult();

        second.Result.ShouldBeFailure(CommonErrors.RequestInProgress);
        (await first).Result.ShouldBeSuccess();
        _harness.Hooks.Handled.ShouldBe(1);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static string NewKey() => Guid.CreateVersion7().ToString();

    private async Task<(Result<CreatedResponse<TicketId>> Result, bool Replayed)> SendAsync(OpenTicketCommand command, string? key)
    {
        await using var work = _harness.Begin(_alice);
        work.Request.IdempotencyKey = key;
        var result = await work.Sender.SendAsync(command, Ct);
        return (result, work.Request.Replayed);
    }

    private async Task InsertProcessingAsync(OpenTicketCommand command, string key, DateTimeOffset leaseExpiresAt)
    {
        var now = _harness.Time.GetUtcNow();
        await using var insert = database.DataSource.CreateCommand("""
            INSERT INTO "Probe"."IdempotencyRecord"
                ("Id", "UserId", "RoleId", "Operation", "Key", "RequestHash", "Status", "LeaseToken", "LeaseExpiresAt", "CreatedAt", "ExpiresAt")
            VALUES ($1, $2, $3, $4, $5, $6, 'Processing', $7, $8, $9, $10)
            """);
        object[] values =
        [
            Guid.CreateVersion7(), _alice.UserId.Value, _alice.RoleId.Value, Operation, key,
            SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(command, IdempotencyJson.Options)),
            Guid.CreateVersion7(), leaseExpiresAt, now.AddMinutes(-5), now.AddHours(24),
        ];
        foreach (var value in values)
        {
            insert.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        await insert.ExecuteNonQueryAsync(Ct);
    }
}
