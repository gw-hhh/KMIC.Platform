namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 游标分页的请求参数
/// </summary>
/// <param name="After">上一页返回的游标；第一页不给</param>
/// <param name="Limit">每页数量，1 到 200，默认 50</param>
public sealed record CursorRequest(string? After = null, int Limit = CursorRequest.DefaultLimit)
{
    /// <summary>
    /// 默认每页数量
    /// </summary>
    public const int DefaultLimit = 50;

    /// <summary>
    /// 每页数量上限
    /// </summary>
    public const int MaxLimit = 200;
}
