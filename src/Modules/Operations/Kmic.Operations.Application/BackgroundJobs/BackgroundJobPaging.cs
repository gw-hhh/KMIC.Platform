using System.Text.Json;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.BackgroundJobs;

/// <summary>
/// 作业的跨模块翻页：归并规则见 <see cref="MergedCursorPaging" />，这里补上提交人、取消人的名称
/// </summary>
internal static class BackgroundJobPaging
{
    public static async Task<Result<CursorPage<BackgroundJobItem>>> PageAsync(
        IEnumerable<IModuleJobStore> stores,
        JobFilter filter,
        CursorRequest request,
        ICursorProtector protector,
        IUserReferences users,
        CancellationToken ct)
    {
        var page = await MergedCursorPaging.PageAsync(
            stores,
            request,
            protector,
            (store, after, take) => store.ListAsync(filter, after is { } position ? new JobPosition(position.At, position.Id) : null, take, ct),
            job => job.SubmittedAt,
            job => job.Id);
        if (page.IsFailure)
        {
            return page.Error;
        }

        var views = page.Value.Items;
        var names = await users.ResolveAsync(
            [.. views.SelectMany(v => new[] { v.SubmittedBy, v.CancelRequestedBy }).OfType<Guid>().Distinct()],
            ct);
        return new CursorPage<BackgroundJobItem>([.. views.Select(v => ToItem(v, names))], page.Value.NextCursor);
    }

    public static BackgroundJobItem ToItem(JobView view, IReadOnlyDictionary<Guid, ReferenceInfo> names) =>
        new(
            view.Id,
            view.Module,
            view.JobType,
            BackgroundJobStatus.FromCode(view.State.ToString()),
            view.SubmittedBy,
            view.SubmittedBy is { } by ? names.GetValueOrDefault(by)?.DisplayName : null,
            view.SubmittedAt,
            view.StartedAt,
            view.FinishedAt,
            view.ProgressDone,
            view.ProgressTotal,
            view.ProgressMessage,
            view.ResultFileId,
            view.ResultSummary is { } summary ? Parse(summary) : null,
            view.ErrorCode,
            view.ErrorMessage,
            view.Attempts,
            view.CancelRequestedAt,
            view.CancelRequestedBy,
            view.CancelRequestedBy is { } canceller ? names.GetValueOrDefault(canceller)?.DisplayName : null);

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
