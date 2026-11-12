namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 游标分页的一页
/// </summary>
/// <typeparam name="T">列表项</typeparam>
/// <param name="Items">这一页的列表项</param>
/// <param name="NextCursor">下一页的游标，作为 After 传回；最后一页为空</param>
public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);
