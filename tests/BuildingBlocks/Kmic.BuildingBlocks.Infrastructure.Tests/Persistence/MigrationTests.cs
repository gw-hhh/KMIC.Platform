using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 迁移：历史表在模块 schema 里，模型没有未生成的迁移，运行账号能读写但不能改表结构
/// </summary>
public sealed class MigrationTests(ClassDatabase<ProbeTemplate> database, ProbeTemplate template) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task History_InModuleSchema()
    {
        await using var command = database.DataSource.CreateCommand("""SELECT count(*) FROM "Probe"."__EFMigrationsHistory" """);

        ((long)(await command.ExecuteScalarAsync(Ct))!).ShouldBe(1);
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

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(Ct);
    }
}
