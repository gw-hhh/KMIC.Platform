using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Kmic.Files.Application.Cleanup;
using Kmic.Files.Application.Uploads.Start;
using Npgsql;
using Shouldly;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 定时清理与删除工作器：到期的文件被标记并删除对象和行，业务正在关联的、还有关联请求在路上的不动，
/// 上传中断的被拒绝，删除失败的退避，运维重试之后删掉
/// </summary>
public sealed class FileCleanupTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(database, template, withWorker: true), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private async Task<int> RunAsync<TCommand>(TCommand command)
        where TCommand : ICommand<ScheduledJobOutcome>
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.ScheduledJob);
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
            "删除工作器删除对象之后把行也删了",
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
        File.Exists(StoragePathOf(key)).ShouldBeFalse("先删对象，再删行");
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

        // 业务命令正在关联这个文件：持有它的共享锁，清理取不到排他锁，这次跳过
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
            (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Available", "共享锁还在，清理跳过");
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

        // 一条关联请求还停在发件箱里（死信不会被自动重试，用它模拟“还在路上”）：这时清理，请求之后落到被清掉的文件上
        var message = Guid.CreateVersion7();
        await seed.SqlAsync(
            """
            INSERT INTO "Sample"."OutboxMessage" ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "SubjectIds", "OccurredAt", "Status", "Attempts")
            VALUES (@message, 'Files.AttachFiles', 1, 0, '{}'::jsonb, '{}'::jsonb, ARRAY[@file]::uuid[], now(), 3, 8)
            """,
            ("message", message), ("file", id));

        await RunAsync(Purge());
        (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Available", "有未完成的关联请求，不能清理");

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
        (await ColumnAsync<string>(seed, stale, "RejectReason")).ShouldBe("上传中断");
        (await ColumnAsync<string>(seed, recent, "Status")).ShouldBe("Uploading");
    }

    [Fact]
    public async Task WhenTheObjectCannotBeDeleted_TheRowStaysAndBacksOff_UntilTheProblemIsGone()
    {
        await using var seed = NewSeed();
        var (id, key) = await ExpiredTemporaryFileAsync(seed);

        // 在对象的位置放一个目录：删文件会失败，模拟存储暂时出问题
        var path = StoragePathOf(key);
        File.Delete(path);
        Directory.CreateDirectory(path);
        await RunAsync(Purge());

        await Eventually.UntilAsync(
            _ => seed.ScalarAsync<int>("""SELECT "DeleteAttempts" FROM "Files"."StoredFile" WHERE "Id" = @id""", ("id", id)),
            attempts => attempts >= 1,
            "删除工作器领取并尝试了一次",
            TimeSpan.FromSeconds(20),
            Ct);
        await Eventually.UntilAsync(
            _ => seed.ScalarAsync<bool>("""SELECT "LeaseToken" IS NULL AND "NextAttemptAt" > now() FROM "Files"."StoredFile" WHERE "Id" = @id""", ("id", id)),
            backedOff => backedOff,
            "失败之后放回队列并退避",
            TimeSpan.FromSeconds(20),
            Ct);
        (await ColumnAsync<string>(seed, id, "Status")).ShouldBe("Deleting", "对象没删掉，行不能删");

        Directory.Delete(path);
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "NextAttemptAt" = now() WHERE "Id" = @id""", ("id", id));
        await WaitForRowGoneAsync(seed, id);
    }

    private async Task<Guid> StartUploadAsync()
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateScope(Alice);
        var started = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new StartUploadCommand(Profile, "没传完.pdf", 100), Ct);
        return started.ShouldBeSuccess().FileId;
    }
}
