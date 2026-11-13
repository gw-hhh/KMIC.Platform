using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;

namespace Kmic.Operations.Application.ScheduledJobs.Runs;

/// <summary>
/// 处理 <see cref="ListScheduledJobRunsQuery"/>
/// </summary>
public sealed class ListScheduledJobRunsQueryHandler(IOperationsQueryDb db, ICursorProtector protector, IUserReferences users)
    : IQueryHandler<ListScheduledJobRunsQuery, CursorPage<ScheduledJobRunItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<CursorPage<ScheduledJobRunItem>>> Handle(ListScheduledJobRunsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = await db.ScheduledJobRuns
            .Where(r => r.JobName == query.JobName)
            .ToCursorPageAsync(query.Cursor, protector, r => r.StartedAt, r => r.Id, descending: true, cancellationToken);
        if (page.IsFailure)
        {
            return page.Error;
        }

        var names = await users.ResolveAsync(ScheduledJobRunItems.UsersOf(page.Value.Items), cancellationToken);
        return new CursorPage<ScheduledJobRunItem>([.. page.Value.Items.Select(r => ScheduledJobRunItems.From(r, names))], page.Value.NextCursor);
    }
}
