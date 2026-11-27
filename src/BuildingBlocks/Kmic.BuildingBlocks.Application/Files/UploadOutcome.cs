namespace Kmic.BuildingBlocks.Application.Files;

public enum UploadOutcome
{
    /// <summary>已写入存储。</summary>
    Stored = 0,

    /// <summary>超出上传大小上限。</summary>
    TooLarge = 1,

    /// <summary>文件类型不支持或校验未通过。</summary>
    TypeNotAllowed = 2,

    Empty = 3,
}
