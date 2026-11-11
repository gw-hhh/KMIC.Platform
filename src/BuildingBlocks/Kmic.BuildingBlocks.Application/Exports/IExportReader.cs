using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Exports;

/// <summary>
/// 导出作业读取数据的入口
/// </summary>
/// <remarks>
/// 在 Worker 专用的流式连接上逐行读取，不把全部数据装进内存，照常套用提交人的数据范围。
/// 导出期间查字典等辅助查询照常用查询上下文，不占这条连接。只能在后台作业里用
/// </remarks>
/// <typeparam name="TQueryDb">模块的查询接口</typeparam>
public interface IExportReader<TQueryDb>
    where TQueryDb : class
{
    /// <summary>
    /// 逐行读取并写出
    /// </summary>
    /// <param name="query">构建查询，和列表查询用同一个方法，导出的就是列表的全部页</param>
    /// <param name="write">写出一行</param>
    /// <param name="ct">取消</param>
    /// <returns>写出的行数；超过行数上限时返回 Common.ExportTooLarge，已写出的部分要丢弃</returns>
    Task<Result<long>> ReadAsync<T>(Func<TQueryDb, IQueryable<T>> query, Func<T, CancellationToken, ValueTask> write, CancellationToken ct);
}
