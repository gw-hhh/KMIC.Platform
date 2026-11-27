using System.Diagnostics.Metrics;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Metrics;
using Kmic.BuildingBlocks.Testing.Waiting;
using Kmic.Files.Application.Cleanup;
using Kmic.Files.Application.Uploads.Start;
using Npgsql;
using Shouldly;

namespace Kmic.Api.Tests.Files;

public sealed class FileCleanupTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(database, template, withWorker: true), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private async Task<int> RunAsync<TCommand>(TCommand command)
        where TCommand : ICommand<ScheduledJobOutcome>
    {
        await using var scope = Factory.Services.AsSystem(CallOrigin.ScheduledJob);
        var outcome = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(command, Ct);
        return outcome.ShouldBeSuccess().ItemsProcessed;
    }

    private static PurgeExpiredFilesCommand Purge() => new(TimeProvider.System.GetUtcNow());

    private async Task<(Guid Id, string Key)> ExpiredTemporaryFileAsync(IdentitySeed seed)
    {
        using var alice = ClientFor(Alice);
        var id = await UploadOkAsync(alice, "到期.pdf", Pdf());
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "ExpiresAt" = now() - interval '1 hour' WHERE "Id" = @id""", ("id", id));
        return (id, (await ColumnAsync<string>(seed, id, "StorageKey"))!);
    }

    private static async Task WaitForRowGoneAsync(IdentitySeed seed, Guid id) =>
        await Eventually.UntilAsync(
            _ => seed.CountAsync($"""SELECT count(*) FROM "Files"."StoredFile" WHERE "Id" = '{id}'"""),
            count => count == 0,
            "删除 Worker 删除存储对象并清理数据库记录",
            TimeSpan.FromSeconds(20),
            Ct);

    [Fact]
    public async Task PurgeExpired_MarksAnExpiredTemporaryFile_AndTheWorkerRemovesItsObjectAndRow()
    {
        await using var seed = NewSeed();
        var (id, key) = await ExpiredTemporaryFileAsync(seed);
        File.Exists(StoragePathOf(key)).ShouldBeTrue();

        var marked = await RunAsync(Purge());

        marked.ShouldBeGreaterThanOrEqualTo(1);
        await WaitForRowGoneAsync(seed, id);
        File.Exists(StoragePathOf(key)).ShouldBeFalse("应先删除存储对象，再删除数据库记录");
    }

    [Fact]
    public async Task PurgeExpired_LeavesAFileThatIsNotDueYet()
    {
        await using var seed = NewSeed();
        using var alice = ClientFor(Alice);
        var id = await UploadOkAsync(alice, "未到期.pdf", Pdf());

        await RunAsync(Purge());

        (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Available");
    }

    [Fact]
    public async Task PurgeExpired_SkipsAFileWhileABusinessCommandHoldsItsSharedLock_AndTakesItAfterwards()
    {
        await using var seed = NewSeed();
        var (id, _) = await ExpiredTemporaryFileAsync(seed);
        using var metrics = new MetricRecorder(Factory.Services.GetRequiredService<IMeterFactory>());

        // 业务命令正在关联该文件并持有共享锁，清理任务无法获取排他锁时应跳过本轮
        await using var data = NpgsqlDataSource.Create(ConnectionString);
        await using (var business = await data.OpenConnectionAsync(Ct))
        await using (var transaction = await business.BeginTransactionAsync(Ct))
        {
            await using (var share = new NpgsqlCommand("SELECT pg_advisory_xact_lock_shared(@key)", business, transaction))
            {
                share.Parameters.AddWithValue("key", LockKeys.Of("Files", id));
                await share.ExecuteNonQueryAsync(Ct);
            }

            await RunAsync(Purge());
            (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Available", "共享锁尚未释放，清理任务应跳过");
            metrics.Of("kmic.files.purge_skipped").ShouldContain(m => m.Tags["reason"] == "locked");
            await transaction.RollbackAsync(Ct);
        }

        await RunAsync(Purge());
        await WaitForRowGoneAsync(seed, id);
    }

    [Fact]
    public async Task PurgeExpired_SkipsAFileThatHasAnUnfinishedAttachRequest_UntilItIsFinished()
    {
        await using var seed = NewSeed();
        var (id, _) = await ExpiredTemporaryFileAsync(seed);

        // 存在滞留于发件箱的关联请求（以死信消息模拟在途请求，因其不会被自动重试）：此时清理会导致后续请求指向已删除文件
        var message = Guid.CreateVersion7();
        await seed.SqlAsync(
            """
            INSERT INTO "Sample"."OutboxMessage" ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "SubjectIds", "OccurredAt", "Status", "Attempts")
            VALUES (@message, 'Files.AttachFiles', 1, 0, '{}'::jsonb, '{}'::jsonb, ARRAY[@file]::uuid[], now(), 3, 8)
            """,
            ("message", message), ("file", id));

        using var metrics = new MetricRecorder(Factory.Services.GetRequiredService<IMeterFactory>());
        await RunAsync(Purge());
        (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Available", "存在未完成的关联请求，不应清理");
        metrics.Of("kmic.files.purge_skipped").ShouldContain(m => m.Tags["reason"] == "attaching");

        await seed.SqlAsync("""UPDATE "Sample"."OutboxMessage" SET "Status" = 2 WHERE "Id" = @message""", ("message", message));
        await RunAsync(Purge());
        await WaitForRowGoneAsync(seed, id);
    }

    [Fact]
    public async Task PurgeExpired_ExpiresARejectedFileWithoutTheProtocol()
    {
        await using var seed = NewSeed();
        using var alice = ClientFor(Alice);
        var response = await UploadAsync(alice, "假的.pdf", "MZ......"u8.ToArray());
        response.StatusCode.ShouldBe((System.Net.HttpStatusCode)415);
        var id = await seed.ScalarAsync<Guid>("""SELECT "Id" FROM "Files"."StoredFile" WHERE "OriginalName" = '假的.pdf' ORDER BY "UploadedAt" DESC LIMIT 1""");
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "ExpiresAt" = now() - interval '1 minute' WHERE "Id" = @id""", ("id", id));

        await RunAsync(Purge());

        await WaitForRowGoneAsync(seed, id);
    }

    [Fact]
    public async Task ResetStaleUploads_RejectsAnUploadThatNeverFinished_ButNotARecentOne()
    {
        await using var seed = NewSeed();
        var stale = await StartUploadAsync();
        var recent = await StartUploadAsync();
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "UploadedAt" = now() - interval '2 hours' WHERE "Id" = @id""", ("id", stale));

        var rejected = await RunAsync(new ResetStaleUploadsCommand(TimeProvider.System.GetUtcNow()));

        rejected.ShouldBeGreaterThanOrEqualTo(1);
        (await ColumnAsync<string>(seed, stale, "Status")).ShouldBe("Rejected");
        (await ColumnAsync<string>(seed, stale, "RejectReason")).ShouldBe("上传超时未完成");
        (await ColumnAsync<string>(seed, recent, "Status")).ShouldBe("Uploading");
    }

    [Fact]
    public async Task WhenTheObjectCannotBeDeleted_TheRowStaysAndBacksOff_UntilTheProblemIsGone()
    {
        await using var seed = NewSeed();
        var (id, key) = await ExpiredTemporaryFileAsync(seed);

        // 在对象路径放置同名目录使文件删除失败，以此模拟存储服务临时异常
        var path = StoragePathOf(key);
        File.Delete(path);
        Directory.CreateDirectory(path);
        await RunAsync(Purge());

        await Eventually.UntilAsync(
            _ => seed.ScalarAsync<int>("""SELECT "DeleteAttempts" FROM "Files"."StoredFile" WHERE "Id" = @id""", ("id", id)),
            attempts => attempts >= 1,
            "删除 Worker 领取任务并完成首次删除尝试",
            TimeSpan.FromSeconds(20),
            Ct);
        await Eventually.UntilAsync(
            _ => seed.ScalarAsync<bool>("""SELECT "LeaseToken" IS NULL AND "NextAttemptAt" > now() FROM "Files"."StoredFile" WHERE "Id" = @id""", ("id", id)),
            backedOff => backedOff,
            "删除失败后重新入队并计算退避时间",
            TimeSpan.FromSeconds(20),
            Ct);
        (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Deleting", "存储对象尚未删除成功，不应删除数据库记录");

        Directory.Delete(path);
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "NextAttemptAt" = now() WHERE "Id" = @id""", ("id", id));
        await WaitForRowGoneAsync(seed, id);
    }

    private async Task<Guid> StartUploadAsync()
    {
        await using var scope = Factory.Services.As(Alice);
        var started = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new StartUploadCommand(Profile, "没传完.pdf", 100), Ct);
        return started.ShouldBeSuccess().FileId;
    }
}
