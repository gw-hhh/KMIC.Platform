namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>页码分页参数；页码从 1 开始，每页数量范围为 1 至 <see cref="MaxPageSize"/>。</summary>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
public sealed record PageRequest(int Page = 1, int PageSize = PageRequest.DefaultPageSize)
{
    public const int DefaultPageSize = 20;

    public const int MaxPageSize = 200;
}
