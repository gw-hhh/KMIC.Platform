namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>游标分页请求参数。</summary>
/// <param name="After">游标，上一页返回的断点</param>
/// <param name="Limit">每页数量</param>
public sealed record CursorRequest(string? After = null, int Limit = CursorRequest.DefaultLimit)
{
    public const int DefaultLimit = 50;

    public const int MaxLimit = 200;
}
