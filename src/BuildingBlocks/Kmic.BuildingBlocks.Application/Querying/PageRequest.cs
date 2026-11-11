namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 页码分页参数
/// </summary>
/// <remarks>
/// 页码从 1 开始；每页数量 1 到 <see cref="MaxPageSize"/>，超出范围由 ToPagedAsync 返回校验错误
/// </remarks>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
public sealed record PageRequest(int Page = 1, int PageSize = PageRequest.DefaultPageSize)
{
    /// <summary>
    /// 默认每页数量
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// 每页数量上限
    /// </summary>
    public const int MaxPageSize = 200;
}
