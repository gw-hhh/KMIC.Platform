using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Application.Dictionaries;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.ContractQueries;

/// <summary>
/// 处理 <see cref="GetDictionarySnapshotQuery"/>
/// </summary>
public sealed class GetDictionarySnapshotQueryHandler(IDictionarySnapshots snapshots) : IQueryHandler<GetDictionarySnapshotQuery, DictionarySnapshot?>
{
    /// <inheritdoc />
    public async ValueTask<Result<DictionarySnapshot?>> Handle(GetDictionarySnapshotQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await snapshots.GetAsync(query.TypeCode, cancellationToken);
    }
}
