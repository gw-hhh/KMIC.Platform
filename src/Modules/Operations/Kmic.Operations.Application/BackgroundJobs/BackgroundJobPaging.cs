using System.Globalization;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.BackgroundJobs;

/// <summary>
/// 跨模块的作业翻页：每个模块各取一批，按提交时间倒序归并
/// </summary>
/// <remarks>
/// 游标是“提交时间加 ID”，每个模块在自己的表里按这个位置往后取，归并后截取；各模块之间按同样的次序排，
/// 所以翻页不重不漏。排序键相同时按 ID，并且要按数据库比较 uuid 的次序（逐字节）排，不能用 .NET 的 Guid 比较：
/// 两者的次序不一样，归并和每个模块自己的次序不一致就会在同一时间的作业之间漏行
/// </remarks>
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
        if (request.Limit is < 1 or > CursorRequest.MaxLimit)
        {
            return CommonErrors.ValidationFailedWith([new FieldError("limit", "Range", $"每页数量必须在 1 到 {CursorRequest.MaxLimit} 之间")]);
        }

        JobPosition? after = null;
        if (request.After is { Length: > 0 } cursor)
        {
            if (!protector.TryUnprotect(cursor, out var key, out var id)
                || !DateTimeOffset.TryParseExact(key, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var at))
            {
                return CommonErrors.ValidationFailedWith([new FieldError("after", "InvalidCursor", "游标无效或已被修改")]);
            }

            after = new JobPosition(at, id);
        }

        var take = request.Limit + 1;
        var batches = await Task.WhenAll(stores.Select(store => store.ListAsync(filter, after, take, ct)));
        var merged = batches
            .SelectMany(batch => batch)
            .OrderByDescending(job => job.SubmittedAt)
            .ThenByDescending(job => job.Id, DatabaseUuidOrder.Instance)
            .Take(take)
            .ToList();
        var hasMore = merged.Count > request.Limit;
        var views = hasMore ? merged.Take(request.Limit).ToList() : merged;
        var names = await users.ResolveAsync(
            [.. views.SelectMany(v => new[] { v.SubmittedBy, v.CancelRequestedBy }).OfType<Guid>().Distinct()],
            ct);
        var next = hasMore ? protector.Protect(views[^1].SubmittedAt.ToString("O", CultureInfo.InvariantCulture), views[^1].Id) : null;
        return new CursorPage<BackgroundJobItem>([.. views.Select(v => ToItem(v, names))], next);
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

    // 数据库比较 uuid 是逐字节的，大端字节序
    private sealed class DatabaseUuidOrder : IComparer<Guid>
    {
        public static DatabaseUuidOrder Instance { get; } = new();

        public int Compare(Guid x, Guid y)
        {
            Span<byte> left = stackalloc byte[16];
            Span<byte> right = stackalloc byte[16];
            x.TryWriteBytes(left, bigEndian: true, out _);
            y.TryWriteBytes(right, bigEndian: true, out _);
            return left.SequenceCompareTo(right);
        }
    }
}
