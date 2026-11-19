using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 文件下载响应
/// </summary>
public static class FileResults
{
    /// <summary>
    /// 把文件流式写成响应，支持 ETag 和 Range（存储给出的流能随机读取时）
    /// </summary>
    public static IResult Stream(FileDownload file, bool inline = false)
    {
        ArgumentNullException.ThrowIfNull(file);
        return new FileDownloadResult(file, inline && FileTypes.OpensInBrowser(file.FileType));
    }
}
