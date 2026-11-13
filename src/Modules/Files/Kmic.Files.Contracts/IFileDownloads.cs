using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 按文件 ID 取下载信息
/// </summary>
/// <remarks>
/// Files 没有自己的下载端点，下载权限完全继承业务对象：业务的下载查询先完成业务对象的授权、确认关联存在，
/// 再用它取下载信息。<b>调用方必须先完成业务对象的授权</b>，知道文件 ID 就能取到下载信息。
/// 文件状态由这里判断，业务不用重复：只有已关联的文件可以下载，被彻底删除的返回 410 Files.Purged，
/// 还没登记完的返回可重试的 Files.NotReady，其余按不存在处理
/// </remarks>
public interface IFileDownloads
{
    /// <summary>
    /// 取一个文件的下载信息
    /// </summary>
    /// <param name="fileId">文件 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<Result<FileDownload>> GetAsync(Guid fileId, CancellationToken ct);
}
