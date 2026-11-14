using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 处理 <see cref="ListRebuildsQuery"/>
/// </summary>
public sealed class ListRebuildsQueryHandler(IReadModelRebuildAdmin rebuilds) : IQueryHandler<ListRebuildsQuery, IReadOnlyList<RebuildItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyList<RebuildItem>>> Handle(ListRebuildsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var runs = await rebuilds.ListAsync(cancellationToken);
        List<RebuildItem> items =
        [
            .. runs.Select(r => new RebuildItem(r.Module, r.RunId, r.Subscription, r.ReadModel, r.JobId, StatusOf(r.State), r.Scanned, r.Missing, r.StartedAt, r.FinishedAt)),
        ];
        return items;
    }

    private static RebuildRunStatus StatusOf(RebuildRunState state) =>
        state switch
        {
            RebuildRunState.Active => RebuildRunStatus.Active,
            RebuildRunState.AwaitingConfirmation => RebuildRunStatus.AwaitingConfirmation,
            RebuildRunState.Completed => RebuildRunStatus.Completed,
            RebuildRunState.Abandoned => RebuildRunStatus.Abandoned,
            _ => RebuildRunStatus.Failed,
        };
}
