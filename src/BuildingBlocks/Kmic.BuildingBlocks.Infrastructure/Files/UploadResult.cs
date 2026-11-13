namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 上传处理的结果：成功时带着写入的文件，失败时只有原因
/// </summary>
/// <param name="Outcome">结果</param>
/// <param name="File">写入的文件；只有 <see cref="UploadOutcome.Stored" /> 时有</param>
public sealed record UploadResult(UploadOutcome Outcome, StoredFileInfo? File)
{
    /// <summary>
    /// 没有写入存储的结果
    /// </summary>
    /// <param name="outcome">原因</param>
    public static UploadResult Refused(UploadOutcome outcome) => new(outcome, null);
}
