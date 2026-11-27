using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.BuildingBlocks.Presentation.Http;

public static class FileResults
{
    /// <summary>以流式方式写入文件响应，支持 ETag 协商与 Range 分段下载</summary>
    public static IResult Stream(FileDownload file, bool inline = false)
    {
        ArgumentNullException.ThrowIfNull(file);
        return new FileDownloadResult(file, inline && FileTypes.OpensInBrowser(file.FileType));
    }
}
