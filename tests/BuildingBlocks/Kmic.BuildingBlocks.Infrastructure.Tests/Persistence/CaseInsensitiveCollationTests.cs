using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 每个测试改坏一处，各用一个新克隆的库
/// </summary>
public sealed class CaseInsensitiveCollationTests(ProbeTemplate template)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Ensure_OnPreparedDatabase_IsIdempotent()
    {
        await using var database = await CloneAsync();
        await using var connection = await database.DataSource.OpenConnectionAsync(Ct);

        await CaseInsensitiveCollation.EnsureAsync(connection, Ct);
        await CaseInsensitiveCollation.EnsureAsync(connection, Ct);
    }

    [Fact]
    public async Task Ensure_ExistingDefinitionDiffers_Fails()
    {
        await using var database = await CloneAsync();
        await using var connection = await database.DataSource.OpenConnectionAsync(Ct);
        await ExecuteAsync(connection, """
            ALTER TABLE "Probe"."Ticket" ALTER COLUMN "Number" TYPE varchar(20) COLLATE "C";
            DROP COLLATION public."CaseInsensitive";
            CREATE COLLATION public."CaseInsensitive" (provider = icu, locale = 'und', deterministic = true);
            """);

        var error = await Should.ThrowAsync<InvalidOperationException>(() => CaseInsensitiveCollation.EnsureAsync(connection, Ct));
        error.Message.ShouldContain("定义不符");
    }

    [Fact]
    public async Task Ensure_SameNameInModuleSchema_Fails()
    {
        await using var database = await CloneAsync();
        await using var connection = await database.DataSource.OpenConnectionAsync(Ct);
        await ExecuteAsync(connection, """CREATE COLLATION "Probe"."CaseInsensitive" (provider = icu, locale = 'und-u-ks-level2', deterministic = false)""");

        var error = await Should.ThrowAsync<InvalidOperationException>(() => CaseInsensitiveCollation.EnsureAsync(connection, Ct));
        error.Message.ShouldContain("schema Probe");
    }

    [Fact]
    public async Task VerifyColumns_BoundToPublicCollation_Passes_OtherwiseFails()
    {
        await using var database = await CloneAsync();
        await using var harness = new ProbeHarness(database.ConnectionString);
        await using var work = harness.BeginAsSystem();
        await using var connection = await database.DataSource.OpenConnectionAsync(Ct);

        await CaseInsensitiveCollation.VerifyColumnsAsync(connection, work.Db, Ct);

        await ExecuteAsync(connection, """ALTER TABLE "Probe"."Ticket" ALTER COLUMN "Number" TYPE varchar(20) COLLATE "C" """);
        var error = await Should.ThrowAsync<InvalidOperationException>(() => CaseInsensitiveCollation.VerifyColumnsAsync(connection, work.Db, Ct));
        error.Message.ShouldContain("\"Probe\".\"Ticket\".\"Number\"");
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<OwnedClone> CloneAsync()
    {
        var clone = new ClassDatabase<ProbeTemplate>(template);
        await clone.InitializeAsync();
        return new OwnedClone(clone);
    }

    private sealed class OwnedClone(ClassDatabase<ProbeTemplate> clone) : IAsyncDisposable
    {
        public NpgsqlDataSource DataSource => clone.DataSource;

        public string ConnectionString => clone.ConnectionString;

        public ValueTask DisposeAsync() => clone.DisposeAsync();
    }
}
