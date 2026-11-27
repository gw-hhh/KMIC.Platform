using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Exports;

/// <summary>导出作业数据流式读取器，仅限在后台作业中调用。</summary>
public interface IExportReader<TQueryDb>
    where TQueryDb : class
{
    /// <summary>逐行读取并写出；成功时返回写出总行数，超过上限时返回 Common.ExportTooLarge（已写出内容须丢弃）。</summary>
    Task<Result<long>> ReadAsync<T>(Func<TQueryDb, IQueryable<T>> query, Func<T, CancellationToken, ValueTask> write, CancellationToken ct);
}
