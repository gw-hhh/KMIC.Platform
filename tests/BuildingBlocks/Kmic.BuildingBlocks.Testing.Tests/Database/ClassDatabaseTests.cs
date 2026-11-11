using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Database;

public sealed class ClassDatabaseTests(ProbeTemplate template)
{
    [Fact]
    public async Task Clone_HasTemplateSchemaAndPinnedServerVersion()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var database = new ClassDatabase<ProbeTemplate>(template);
        await database.InitializeAsync();
        try
        {
            await using var command = database.DataSource.CreateCommand("""SELECT current_setting('server_version'), (SELECT count(*) FROM "Probe")""");
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);

            reader.GetString(0).ShouldStartWith("18.6");
            reader.GetInt64(1).ShouldBe(0);
        }
        finally
        {
            await database.DisposeAsync();
        }
    }

    [Fact]
    public async Task Dispose_DropsTheClone()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var database = new ClassDatabase<ProbeTemplate>(template);
        await database.InitializeAsync();
        var name = database.Name;

        // 故意留一个打开的连接，删除库时要能强制断开
        await using var open = await database.DataSource.OpenConnectionAsync(cancellationToken);
        await database.DisposeAsync();

        (await DatabaseExistsAsync(name, cancellationToken)).ShouldBeFalse();
    }

    private async Task<bool> DatabaseExistsAsync(string name, CancellationToken cancellationToken)
    {
        await using var admin = NpgsqlDataSource.Create(template.ConnectionString("postgres", pooling: false));
        await using var command = admin.CreateCommand("SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = $1)");
        command.Parameters.Add(new NpgsqlParameter { Value = name });
        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }
}
