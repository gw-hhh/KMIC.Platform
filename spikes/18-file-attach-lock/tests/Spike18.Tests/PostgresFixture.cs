using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Spike18.Tests;

// 独占的临时实例：测试会修改实例级的同步复制设置。
// 普通连接一律 synchronous_commit = local；只有被测的业务事务显式改为 on，才会停在“等待同步复制”
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string Schema = """
        CREATE SCHEMA files;
        CREATE TABLE files."StoredFile" (
            "Id" uuid PRIMARY KEY,
            "Status" varchar(20) NOT NULL,
            "ExpiresAt" timestamptz NULL);
        CREATE TABLE files."InboxMessage" (
            "Id" uuid PRIMARY KEY,
            "MessageId" uuid NOT NULL UNIQUE,
            "Status" smallint NOT NULL,
            "SubjectIds" uuid[] NULL);
        CREATE INDEX ON files."InboxMessage" USING gin ("SubjectIds") WHERE "SubjectIds" IS NOT NULL AND "Status" IN (0, 1, 3);

        CREATE SCHEMA quotation;
        CREATE TABLE quotation."QuotationAttachment" (
            "Id" uuid PRIMARY KEY,
            "QuotationId" uuid NOT NULL,
            "FileId" uuid NOT NULL);
        CREATE TABLE quotation."OutboxMessage" (
            "Id" uuid PRIMARY KEY,
            "MessageType" varchar(200) NOT NULL,
            "Status" smallint NOT NULL,
            "SubjectIds" uuid[] NULL);
        CREATE INDEX ON quotation."OutboxMessage" USING gin ("SubjectIds") WHERE "SubjectIds" IS NOT NULL AND "Status" IN (0, 1, 3);
        """;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public NpgsqlDataSource Source { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);
        var builder = new NpgsqlDataSourceBuilder(_container.GetConnectionString());
        builder.ConnectionStringBuilder.Options = "-c synchronous_commit=local";
        Source = builder.Build();
        await Db.ExecAsync(Source, Schema);
    }

    public async ValueTask DisposeAsync()
    {
        await Source.DisposeAsync();
        await _container.DisposeAsync();
    }

    // 要求一个不存在的同步备库：之后 synchronous_commit = on 的事务提交时一直等待
    public Task RequireMissingStandbyAsync() => SetStandbyNamesAsync("kmic_missing_standby");

    // 清空后，正在等待的事务立即完成提交
    public Task ReleaseStandbyWaitAsync() => SetStandbyNamesAsync(string.Empty);

    private async Task SetStandbyNamesAsync(string names)
    {
        await using var connection = await Source.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await Db.ExecAsync(connection, null, $"ALTER SYSTEM SET synchronous_standby_names = '{names}'");
        await Db.ExecAsync(connection, null, "SELECT pg_reload_conf()");
    }
}
