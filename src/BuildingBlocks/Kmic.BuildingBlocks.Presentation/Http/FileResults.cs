using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 文件下载响应
/// </summary>
/// <remarks>
/// 业务的下载查询授权、确认关联之后返回 <see cref="FileDownload" />，端点用这里把它写成响应。
/// 不存在“知道文件 ID 就能下载”的路径：存储键只出现在这个响应里，不返回给任何客户端
/// </remarks>
public static class FileResults
{
    /// <summary>
    /// 把文件流式写成响应，支持 ETag 和 Range（存储给出的流能随机读取时）
    /// </summary>
    /// <param name="file">下载信息</param>
    /// <param name="inline">在浏览器里直接打开；只有 PDF 和图片可以，其余类型仍然作为附件下载</param>
    public static IResult Stream(FileDownload file, bool inline = false)
    {
        ArgumentNullException.ThrowIfNull(file);
        return new FileDownloadResult(file, inline && FileTypes.OpensInBrowser(file.FileType));
    }
}
