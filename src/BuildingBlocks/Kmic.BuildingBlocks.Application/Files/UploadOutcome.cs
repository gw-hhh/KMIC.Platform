namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 一次上传处理的结果
/// </summary>
public enum UploadOutcome
{
    /// <summary>
    /// 已写入存储
    /// </summary>
    Stored = 0,

    /// <summary>
    /// 超过上传档案的大小上限，没有写入存储
    /// </summary>
    TooLarge = 1,

    /// <summary>
    /// 扩展名不认识、文件头与扩展名不符，或者上传档案不允许这种类型，没有写入存储
    /// </summary>
    TypeNotAllowed = 2,

    /// <summary>
    /// 文件是空的，没有写入存储
    /// </summary>
    Empty = 3,
}
