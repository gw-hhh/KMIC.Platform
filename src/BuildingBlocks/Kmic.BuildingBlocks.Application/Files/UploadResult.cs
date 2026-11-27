namespace Kmic.BuildingBlocks.Application.Files;

/// <param name="File">已存储的文件信息；仅当结果为 <see cref="UploadOutcome.Stored" /> 时非空。</param>
public sealed record UploadResult(UploadOutcome Outcome, StoredFileInfo? File)
{
    /// <summary>构造未写入存储的拒绝结果。</summary>
    public static UploadResult Refused(UploadOutcome outcome) => new(outcome, null);
}
