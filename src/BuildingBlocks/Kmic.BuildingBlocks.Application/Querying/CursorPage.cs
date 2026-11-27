namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>游标分页结果页；<paramref name="NextCursor"/> 在末页时为 null。</summary>
public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);
