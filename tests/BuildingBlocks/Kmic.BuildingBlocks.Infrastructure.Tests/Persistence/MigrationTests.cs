using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

public sealed class MigrationTests(ClassDatabase<ProbeTemplate> database, ProbeTemplate template) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task History_InModuleSchema()
    {
        await using var harness = new ProbeHarness(database.ConnectionString);
        await using var work = harness.BeginAsSystem();
        await using var command = database.DataSource.CreateCommand("""SELECT count(*) FROM "Probe"."__EFMigrationsHistory" """);

        ((long)(await command.ExecuteScalarAsync(Ct))!).ShouldBe(work.Db.Database.GetMigrations().Count());
    }

    [Fact]
    public async Task Model_HasNoPendingChanges()
    {
        await using var harness = new ProbeHarness(database.ConnectionString);
        await using var work = harness.BeginAsSystem();

        work.Db.Database.HasPendingModelChanges().ShouldBeFalse();
        (await work.Db.Database.GetPendingMigrationsAsync(Ct)).ShouldBeEmpty();
    }

    [Fact]
    public async Task RuntimeAccount_ReadsAndWrites_CannotChangeSchema()
    {
        await using var connection = new NpgsqlConnection(template.RuntimeConnectionString(database.Name));
        await connection.OpenAsync(Ct);

        await ExecuteAsync(connection, """SELECT count(*) FROM "Probe"."Ticket" """);
        await ExecuteAsync(connection, """DELETE FROM "Probe"."IdempotencyRecord" WHERE false""");

        var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(connection, """CREATE TABLE "Probe"."Intruder" ("Id" int)"""));
        error.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    public async Task RuntimeAccount_AppendOnlyTable_InsertsAndReadsButCannotUpdateOrDelete()
    {
        await using var connection = new NpgsqlConnection(template.RuntimeConnectionString(database.Name));
        await connection.OpenAsync(Ct);

        await ExecuteAsync(
            connection,
            """
            INSERT INTO "Probe"."ReviewStateHistory" ("Id", "AggregateId", "StateName", "ToCode", "ActionCode", "OccurredAt", "TraceId")
            VALUES (gen_random_uuid(), gen_random_uuid(), 'Status', 'Draft', 'Create', now(), '00000000000000000000000000000000')
            """);
        await ExecuteAsync(connection, """SELECT count(*) FROM "Probe"."ReviewStateHistory" """);

        foreach (var sql in new[] { """UPDATE "Probe"."ReviewStateHistory" SET "Reason" = 'x'""", """DELETE FROM "Probe"."ReviewStateHistory" """ })
        {
            var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(connection, sql));
            error.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
        }
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(Ct);
    }
}
