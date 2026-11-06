using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Spike16.Tests;

// 来源模块 customer 发布消息；quotation、orders 两个模块订阅。表结构只保留 05-07 中与本原型有关的列
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string Schema = """
        CREATE SCHEMA customer;
        CREATE TABLE customer."OutboxMessage" (
            "Id" uuid PRIMARY KEY,
            "Seq" bigint GENERATED ALWAYS AS IDENTITY,
            "MessageType" varchar(200) NOT NULL,
            "Payload" jsonb NOT NULL,
            "Status" smallint NOT NULL,
            "Attempts" int NOT NULL DEFAULT 0,
            "NextAttemptAt" timestamptz NOT NULL,
            "LeaseToken" uuid NULL,
            "LeaseUntil" timestamptz NULL,
            "LastError" varchar(4000) NULL,
            "DeliveredAt" timestamptz NULL);
        CREATE TABLE customer."Customer" ("Id" uuid PRIMARY KEY, "Version" bigint NOT NULL, "Name" text NOT NULL);

        CREATE SCHEMA quotation;
        CREATE TABLE quotation."InboxMessage" (
            "Id" uuid PRIMARY KEY,
            "MessageId" uuid NOT NULL,
            "Subscription" varchar(200) NOT NULL,
            "SourceModule" varchar(50) NOT NULL,
            "Payload" jsonb NOT NULL,
            "Status" smallint NOT NULL,
            "Attempts" int NOT NULL DEFAULT 0,
            "NextAttemptAt" timestamptz NOT NULL,
            "LeaseToken" uuid NULL,
            "LeaseUntil" timestamptz NULL,
            "LastError" varchar(4000) NULL,
            "ProcessedAt" timestamptz NULL,
            UNIQUE ("MessageId", "Subscription"));
        CREATE TABLE quotation."Effect" ("MessageId" uuid NOT NULL);
        CREATE TABLE quotation."CustomerSummary" (
            "Id" uuid PRIMARY KEY,
            "SourceVersion" bigint NOT NULL,
            "Name" text NOT NULL,
            "IsTerminal" boolean NOT NULL DEFAULT false,
            "TerminalReason" smallint NULL,
            "TerminalAt" timestamptz NULL,
            "RebuildRunId" uuid NULL,
            "SyncedAt" timestamptz NOT NULL DEFAULT clock_timestamp());
        CREATE TABLE quotation."ReadModelRebuildRun" (
            "RunId" uuid PRIMARY KEY,
            "ReadModel" varchar(200) NOT NULL,
            "Status" varchar(30) NOT NULL);
        CREATE UNIQUE INDEX "UX_ReadModelRebuildRun_Active" ON quotation."ReadModelRebuildRun" ("ReadModel")
            WHERE "Status" IN ('Running', 'PendingConfirmation');

        CREATE SCHEMA orders;
        CREATE TABLE orders."InboxMessage" (LIKE quotation."InboxMessage" INCLUDING ALL);
        CREATE TABLE orders."Effect" ("MessageId" uuid NOT NULL);
        """;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public NpgsqlDataSource Source { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);
        Source = NpgsqlDataSource.Create(_container.GetConnectionString());
        await Db.ExecAsync(Source, Schema);
    }

    public async ValueTask DisposeAsync()
    {
        await Source.DisposeAsync();
        await _container.DisposeAsync();
    }
}
