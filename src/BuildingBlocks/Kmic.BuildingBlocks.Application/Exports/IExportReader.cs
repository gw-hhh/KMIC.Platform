using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Exports;

/// <summary>
/// 导出作业读取数据的入口，逐行流式读取
/// </summary>
/// <remarks>
/// 只能在后台作业里用
/// </remarks>
/// <typeparam name="TQueryDb">模块的查询接口</typeparam>
public interface IExportReader<TQueryDb>
    where TQueryDb : class
{
    /// <summary>
    /// 逐行读取并写出
    /// </summary>
    /// <returns>写出的行数；超过行数上限时返回 Common.ExportTooLarge，已写出的部分要丢弃</returns>
    Task<Result<long>> ReadAsync<T>(Func<TQueryDb, IQueryable<T>> query, Func<T, CancellationToken, ValueTask> write, CancellationToken ct);
}
