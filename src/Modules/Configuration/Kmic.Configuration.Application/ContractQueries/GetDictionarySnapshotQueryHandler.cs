using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Application.Dictionaries;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.ContractQueries;

/// <summary>
/// 读取字典快照
/// </summary>
public sealed class GetDictionarySnapshotQueryHandler(IDictionarySnapshots snapshots) : IQueryHandler<GetDictionarySnapshotQuery, DictionarySnapshot?>
{
    /// <inheritdoc />
    public async ValueTask<Result<DictionarySnapshot?>> Handle(GetDictionarySnapshotQuery query, CancellationToken cancellationToken)
    {
        return await snapshots.GetAsync(query.TypeCode, cancellationToken);
    }
}
