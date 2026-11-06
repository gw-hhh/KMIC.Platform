using Npgsql;
using Xunit;

namespace Spike05.Tests;

// 原型只用原生 SQL：兼容性检查直接执行 SQL
internal static class Db
{
    public static async Task<int> ExecAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql, params (string Name, object? Value)[] parameters)
    {
        await using var command = Create(connection, transaction, sql, parameters);
        return await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }

    public static async Task<int> ExecAsync(NpgsqlDataSource db, string sql, params (string Name, object? Value)[] parameters)
    {
        await using var connection = await db.OpenConnectionAsync(TestContext.Current.CancellationToken);
        return await ExecAsync(connection, null, sql, parameters);
    }

    public static async Task<List<object?[]>> QueryAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql, params (string Name, object? Value)[] parameters)
    {
        await using var command = Create(connection, transaction, sql, parameters);
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        var rows = new List<object?[]>();
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            var row = new object?[reader.FieldCount];
            for (var i = 0; i < row.Length; i++)
            {
                row[i] = await reader.IsDBNullAsync(i, TestContext.Current.CancellationToken) ? null : reader.GetValue(i);
            }

            rows.Add(row);
        }

        return rows;
    }

    public static async Task<List<object?[]>> QueryAsync(NpgsqlDataSource db, string sql, params (string Name, object? Value)[] parameters)
    {
        await using var connection = await db.OpenConnectionAsync(TestContext.Current.CancellationToken);
        return await QueryAsync(connection, null, sql, parameters);
    }

    public static async Task<T> ScalarAsync<T>(NpgsqlDataSource db, string sql, params (string Name, object? Value)[] parameters)
    {
        var rows = await QueryAsync(db, sql, parameters);
        return (T)rows[0][0]!;
    }

    private static NpgsqlCommand Create(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql, (string Name, object? Value)[] parameters)
    {
        var command = new NpgsqlCommand(sql, connection, transaction);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        return command;
    }
}
