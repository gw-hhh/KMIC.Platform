using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Files.Application.Administration;

public static class FileSort
{
    /// <summary>默认按文件登记时间倒序</summary>
    public static readonly SortMap<FileListItem> Map = SortMap<FileListItem>
        .Field("uploadedAt", x => x.UploadedAt)
        .Field("name", x => x.Name)
        .Field("size", x => x.Size)
        .Default("-uploadedAt");
}
