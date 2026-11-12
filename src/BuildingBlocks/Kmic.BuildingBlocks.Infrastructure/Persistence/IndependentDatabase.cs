using System.Data.Common;
using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed class IndependentDatabase(DataSources dataSources, ExecutionScope execution) : IIndependentDatabase
{
    public async Task<TResult> InTransactionAsync<TResult>(Func<DbConnection, DbTransaction, CancellationToken, Task<TResult>> work, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(work);

        await using var connection = await dataSources.Reads(execution.EntryOrigin).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        var result = await work(connection, transaction, ct);
        await transaction.CommitAsync(ct);
        return result;
    }
}
