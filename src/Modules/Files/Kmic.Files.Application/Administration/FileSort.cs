using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 文件列表允许排序的字段
/// </summary>
public static class FileSort
{
    /// <summary>
    /// 排序字段表，默认按上传时间倒序
    /// </summary>
    public static readonly SortMap<FileListItem> Map = SortMap<FileListItem>
        .Field("uploadedAt", x => x.UploadedAt)
        .Field("name", x => x.Name)
        .Field("size", x => x.Size)
        .Default("-uploadedAt");
}
